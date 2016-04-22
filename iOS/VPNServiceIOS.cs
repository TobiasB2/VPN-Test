using System.Threading;
using NetworkExtension;
using Security;
using Foundation;
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using UIKit;
using VPNTest;


/*
Sources:
Establish VPN connection:
http://ramezanpour.net/post/2014/08/03/configure-and-manage-vpn-connections-programmatically-in-ios-8/
KeyChain Concept:
https://developer.apple.com/library/mac/documentation/Security/Conceptual/keychainServConcepts/02concepts/concepts.html
Xamarin KeyChain Example:
https://github.com/xamarin/monotouch-samples/tree/master/Keychain
Persistent KeyChain iOS:
http://ramezanpour.net/post/2014/09/26/how-to-get-persistent-references-to-keychain-items-in-ios/

*/

[assembly: Xamarin.Forms.Dependency (typeof(VPNServiceIOS))]
namespace VPNTest
{
	public class VPNServiceIOS : IVPNService
	{
		private NEVpnManager manager;


		public VPNServiceIOS ()
		{
			manager = NEVpnManager.SharedManager;
			manager.LoadFromPreferences (error => {
				if (error != null) {
					Console.WriteLine ("Error loading VPN preferences: ");
					Console.WriteLine (error);
				}
			});
		}

		public void AddConfig() {

			// The password for the VPN connection, add this to Keychain
			var password = new SecRecord (SecKind.GenericPassword) {
				Service = "Password Service",
				ValueData = NSData.FromString ("MY_PASSWORD", NSStringEncoding.UTF8),
				Generic = NSData.FromString ("VPNPas", NSStringEncoding.UTF8),
			};

			// The query for the VPN password. Use this to find the password in Keychain
			var queryPassword = new SecRecord (SecKind.GenericPassword) {
				Service = "Password Service",
				Generic = NSData.FromString ("VPNPas", NSStringEncoding.UTF8),
			};

			// The shared secret for the VPN connection, add this to Keychain
			var secret = new SecRecord (SecKind.GenericPassword) {
				Service = "Secret Service",
				ValueData = NSData.FromString ("hide.io", NSStringEncoding.UTF8),
				Generic = NSData.FromString ("secret", NSStringEncoding.UTF8),
			};

			// The query for the VPN shared secret. Use this to find the shared secret in Keychain
			var querySecret = new SecRecord (SecKind.GenericPassword) {
				Service = "Secret Service",
				Generic = NSData.FromString ("secret", NSStringEncoding.UTF8),
			};

			// First remove old Keychain entries, then add the new ones
			// Just for testing purposes: this is to make sure the keychain entries are correct
			var err = SecKeyChain.Remove (queryPassword);
			Console.WriteLine ("Password remove: " + err);

			err = SecKeyChain.Remove (querySecret);
			Console.WriteLine ("Secret remove: " + err);

			err = SecKeyChain.Add (password);
			Console.WriteLine ("Password add: " + err);

			err = SecKeyChain.Add (secret);
			Console.WriteLine ("Secret add: " + err);


			manager.LoadFromPreferences (error => {
				if (error != null) {
					Console.WriteLine ("Error loading preferences: ");
					Console.WriteLine (error);
				} else {
					NEVpnProtocol p = null;

					// IKEv2 Protocol
					NEVpnProtocolIke2 ike2 = new NEVpnProtocolIke2 ();
					ike2.AuthenticationMethod = NEVpnIkeAuthenticationMethod.None;
				//	ike2.LocalIdentifier = "";
					ike2.RemoteIdentifier = "hide.me";
					ike2.UseExtendedAuthentication = true;
					ike2.DisconnectOnSleep = false;

					// ipSec Protocol
					NEVpnProtocolIpSec ipSec = new NEVpnProtocolIpSec ();
					ipSec.AuthenticationMethod = NEVpnIkeAuthenticationMethod.SharedSecret;
					ipSec.UseExtendedAuthentication = true;
					ipSec.DisconnectOnSleep = false;
					SecStatusCode res;

					// Set the shared secret reference for ipSec: 
					// 1) Search for the secret in keychain and retrieve it as a persistent reference
					// 2) Set the found secret to SharedSecretReference if the secret was found
					var match = SecKeyChain.QueryAsData (querySecret, true, out res);
					if (res == SecStatusCode.Success) {
						Console.WriteLine ("Secret found, setting secret...");
						ipSec.SharedSecretReference = match;
					} else {
						Console.WriteLine ("Could not set secret:");
						Console.WriteLine (res);
					}

					// Set the protocol to IKEv2 or ipSec
			//		p = ike2;
					p = ipSec;

					// Set Accountname, Servername and description
					p.Username = "MY_ACCOUNT";
					p.ServerAddress = "free-nl.hide.me";
					manager.LocalizedDescription = "hide.me VPN";

					// Set the password reference for the protocol: 
					// 1) Search for the password in keychain and retrieve it as a persistent reference
					// 2) Set the found password to PasswordReference if the secret was found
					match = SecKeyChain.QueryAsData(queryPassword, true, out res);
					if (res == SecStatusCode.Success){
						Console.WriteLine ("Password found, setting password...");
						p.PasswordReference = match;
					}
					else {
						Console.WriteLine (res);
					}
					manager.OnDemandEnabled = false;

					// Set the managers protocol and save it to the iOS custom VPN preferences
					manager.ProtocolConfiguration = p;
					manager.SaveToPreferences (error2 => {
						if (error2 != null) {
							Console.WriteLine ("Could not save VPN preferences");
							Console.WriteLine (error2.DebugDescription);
						} 
					});
				}
			});
		}

		// Start the connection, make sure to load the preferences first
		public void StartVPNConnection ()
		{
			manager.LoadFromPreferences (error => {
				if (error != null) {
					Console.WriteLine ("Could not load preferences: ");
					Console.WriteLine (error.ToString ());
				}
			});
			NSError error2;
			manager.Connection.StartVpnTunnel (out error2);
			if (error2 != null) {
				Console.WriteLine ("Could not establish connection:");
				Console.WriteLine (error2.ToString ());
			}
		}


		// Stop current VPN connection
		public void StopVPNConnection ()
		{
			manager.Connection.StopVpnTunnel ();
		}

		// remove the custom VPN connection from the iOS settings
		public void RemoveVPNConnection()
		{
			manager.RemoveFromPreferences ((NSError error) => {
				if (error != null) {
					Console.WriteLine ("Cannot delete VPN preferences:");
					Console.WriteLine (error.ToString ());
				} 
			});
		}
	}
}
