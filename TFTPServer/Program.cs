using System.Net;
using Tftp.Net;

namespace TFTPServer;

public static class Program
{
    private static string _serverDirectory = null!; 

    public static void Main()
    {
        _serverDirectory = Environment.CurrentDirectory + "/images/";
        
        Console.WriteLine($"Listening for files on {_serverDirectory}");
        using var server = new TftpServer();
        server.OnReadRequest += ServerOnReadRequest;
        server.OnWriteRequest += ServerOnWriteRequest;
        server.OnError += ServerOnError;
        server.Start();
        while (true)
        { 
            
        }
    }

    private static void ServerOnError(TftpTransferError error)
    {
        //Debug purposes
        Console.WriteLine(error.ToString());
    }

    private static void ServerOnWriteRequest(ITftpTransfer transfer, EndPoint client)
    {
        //This should never happen
        Console.WriteLine($"Someone ({client}) is trying to write on the server. This should NEVER happen unless someone" +
                          $" is trying weird things. Transfer cancelled.");
        transfer.Cancel(new TftpErrorPacket(errorCode: 66, "Writing is not enabled on this server"));
        transfer.Dispose();
    }

    private static void ServerOnReadRequest(ITftpTransfer transfer, EndPoint client)
    {
        var path = _serverDirectory + transfer.Filename;
        var file = new FileInfo(path);

        if(file.FullName == "/debian-installer/arm64/grub/grub.cfg")
        {
            file = new FileInfo("/app/images/aarch64/grub.cfg");
        }
        
        if (!file.Exists)
        {
            Console.WriteLine($"A client requested {file.FullName} but there's no file there.");
            ServerOnReadRequestRPI(transfer, client);
        }
        else
        {
            Console.WriteLine($"A client requested {file.FullName}");
            OutputTransferStatus(transfer, "Accepting request from " + client);
            StartTransfer(transfer, new FileStream(file.FullName, FileMode.Open, FileAccess.Read));
        }
    }

    private static void ServerOnReadRequestRPI(ITftpTransfer transfer, EndPoint client)
    {
        var path = _serverDirectory + "boot/" + transfer.Filename;
        var file = new FileInfo(path);
        
        if (!file.Exists)
        {
            Console.WriteLine($"A client (Possible RPI) requested {file.FullName} but there's no file there.");
            CancelTransfer(transfer, TftpErrorPacket.FileNotFound);
        }
        else
        {
            Console.WriteLine($"A client requested {file.FullName}");
            OutputTransferStatus(transfer, "Accepting request from " + client);
            StartTransfer(transfer, new FileStream(file.FullName, FileMode.Open, FileAccess.Read));
        }
    }

    private static void StartTransfer(ITftpTransfer transfer, FileStream fileStream)
    {
        transfer.OnError += OnTransferError;
        transfer.OnFinished += OnTransferFinished;
        transfer.Start(fileStream);
    }

    private static void OnTransferFinished(ITftpTransfer transfer)
    {
        OutputTransferStatus(transfer, "Finished");
    }

    private static void OnTransferError(ITftpTransfer transfer, TftpTransferError error)
    {
        OutputTransferStatus(transfer, "Error: " + error);
    }

    private static void OutputTransferStatus(ITftpTransfer transfer, string message)
    {
        Console.WriteLine($"[ {transfer.Filename} ] {message}");
    }

    private static void CancelTransfer(ITftpTransfer transfer, TftpErrorPacket reason)
    {
        OutputTransferStatus(transfer, "Cancelling transfer: " + reason.ErrorMessage);
        transfer.Cancel(reason);
    }
}