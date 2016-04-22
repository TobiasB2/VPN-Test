using System;

namespace VPNTest
{
	public interface IVPNService
	{
		void StartVPNConnection();
		void StopVPNConnection();
		void RemoveVPNConnection();
		void AddConfig();
	}
}

