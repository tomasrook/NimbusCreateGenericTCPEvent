using System;
using System.Collections.Generic;
using System.Net.Sockets;

namespace NimbusCreateGenericTCPEvent
{
  class NimbusCreateGenericTCPEvent
  {

    //
    // Date     / Sign /  Vers   / Comment
    // ------------------------------------------------------------------------------------------------------------------------------
    // 23.12.06 /  TR  / 3.00.00 / Converted from NimbusCreateEvent and updated to .NET6
    //
    //
    //
    //
    // ------------------------------------------------------------------------------------------------------------------------------
    //
    // Make a self contained application for Ubuntu using following command from a Tools -> Command Line -> Developer Command Prompt
    //
    // dotnet publish -c release -r ubuntu.16.04-x64 --self-contained
    //
    // No need for installing .NET 6
    //
    // OBS! .Net Core is only supported on 64-bit OS's
    //
    // More info:
    //
    // https://stackoverflow.com/questions/46843863/how-to-run-a-net-core-console-application-on-linux
    //


    private static Dictionary<string, string> arguments = new Dictionary<string, string>(StringComparer.InvariantCultureIgnoreCase);

    private enum ReturnCodes
		{
      SentOk = 0,
      ArgumentFailed = 1,
      CouldNotConnect = 2
		}

    static int Main(string[] args)
    {

      ReturnCodes returnCode;

      for (int index = 0; index < args.Length - 1; index += 2)
      {
        if (arguments.ContainsKey(args[index]) == false)
        {
          arguments.Add(args[index], args[index + 1]);
        }
      }

      if (arguments.ContainsKey("ServerAddress") == false || arguments.ContainsKey("Status") == false)
      {
        // Might fail if output is redirected
        try
        {
          Console.WriteLine("NimbusCreateGenericTCPEvent will create an event and transfer it as a Mimbus Generic TCP packet to the Nimbus Server.");
          Console.WriteLine("Ensure you have configured SCADA Import 'Generic TCP (Server)'");
          Console.WriteLine("No check is done to see wheter Nimbus Server is running or not.");
          Console.WriteLine("");
          Console.WriteLine("Usage:");
          Console.WriteLine("");
          Console.WriteLine("NimbusCreateGenericTCPEvent ServerAddress xxx.xxx.xxx.xxx[:ppppp] Status n [Date yyyy-mm-dd] [Time hh:mm:ss] [T0 xxx] [T1 xxx] [T2 xxx] [T3 xxx] [T4 xxx] [T5 xxx]");
          Console.WriteLine("");
          Console.WriteLine("Where:");
          Console.WriteLine("");
          Console.WriteLine("xxx.xxx.xxx.xxx is the Nimbus Server IP v4 address (or DNS name)");
          Console.WriteLine("ppppp is the portnumber (defaults to 15000), ensure the port is allowed though any firewall (Windows or physical)");
          Console.WriteLine("n is 0 = Inactive, 1 = Active, 2 = Acked, 3 = Send text message");
          Console.WriteLine("For Alarms (n = 0..2): T0 = Tag, T1 = Area, T2 = Category, T3 = Name, T4 = Description, T5 = State");
          Console.WriteLine("For Text Message (n = 3): T3 = Name of receiver, T4 = Text to send");
          Console.WriteLine("Texts containing space must be wrapped in quotes (\")");
          Console.WriteLine("");
          Console.WriteLine("Examples (Windows, for Linux put ./ before file name and ensure it has execution rights):");
          Console.WriteLine("NimbusCreateGenericTCPEvent serveraddress nimbus.myorg.se status 1 t0 TA0220GT81 t4 \"Freeze protection TA0220\"");
          Console.WriteLine("NimbusCreateGenericTCPEvent serveraddress 127.0.0.1:15000 status 3 t3 \"nisse hult (SMS)\" t4 \"Time to have some food\"");
          Console.WriteLine("");

#if DEBUG
          Console.ReadKey();
#endif
        }
        catch
        {
        }

        returnCode = ReturnCodes.ArgumentFailed;

      }
      else
      {

        TcpClient tcpClient = null;
        NetworkStream networkStream = null;

        try
        {
          string[] serveraddress = GetArgumentAsString("ServerAddress").Split(':');

          UInt16 portNumber;

          tcpClient = new TcpClient();

          if (serveraddress.Length > 1)
          {
            if (UInt16.TryParse(serveraddress[1], out portNumber) == false)
            {
              portNumber = 15000;
            }
          }
          else
          {
            portNumber = 15000;
          }

          tcpClient.Connect(serveraddress[0], portNumber);
          networkStream = tcpClient.GetStream();

          string packet = GetArgumentAsString("Status").Replace("|", "").Trim() + "|" +
            GetArgumentAsString("Date").Replace("|", "").Trim() + "|" +
            GetArgumentAsString("Time").Replace("|", "").Trim() + "|" +
            GetArgumentAsString("T0").Replace("|", "") + "|" +
            GetArgumentAsString("T1").Replace("|", "") + "|" +
            GetArgumentAsString("T2").Replace("|", "") + "|" +
            GetArgumentAsString("T3").Replace("|", "") + "|" +
            GetArgumentAsString("T4").Replace("|", "") + "|" +
            GetArgumentAsString("T5").Replace("|", "");

          networkStream.Write(System.Text.Encoding.GetEncoding("ISO-8859-1").GetBytes(packet + "\n"));

          try
          {
            Console.WriteLine("Sent event to Nimbus: " + packet);
          }
          catch
          {
          }

          returnCode = ReturnCodes.SentOk;

        }
        catch (Exception exc)
        {
          try
          {
            Console.WriteLine("Failed to write packet to Nimbus, error: " + exc.Message);
          }
          catch
          {
          }

          returnCode = ReturnCodes.CouldNotConnect;

        }

        if (networkStream != null)
        {
          try
          {
            networkStream.Close(500);
          }
          catch
          {
          }
        }
        if (tcpClient != null)
        {
          try
          {
            tcpClient.Close();
          }
          catch
          {
          }
        }
      }
#if DEBUG
      Console.ReadKey();
#endif

      return (int)returnCode;

    }

    private static string GetArgumentAsString(string argument)
    {
      return (arguments.ContainsKey(argument) ? arguments[argument] : "");
    }
  }
}
