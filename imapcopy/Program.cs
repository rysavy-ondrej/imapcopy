using MailKit.Net.Imap;
using System;
using System.Linq;

namespace imapcopy
{
    /// <summary>
    /// imapcopy.exe SOURCE-IMAP TARGET-IMAP
    /// </summary>
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length != 2)
            {
                Console.WriteLine("Usage: imapcopy.exe SOURCE-IMAP TARGET-IMAP");
                return;
            }
            var sourceImapServer = args[0];
            var targetImapServer = args[1];

            Console.Write($"Username for {sourceImapServer}:");
            var sourceUsername = Console.ReadLine();
            Console.Write($"Password:");
            var sourcePassword = SecureConsole.ReadPlainPassword();

            Console.Write($"Username for {targetImapServer}:");
            var targetUsername = Console.ReadLine();
            Console.Write($"Password:");
            var targetPassword = SecureConsole.ReadPlainPassword();

            Console.WriteLine($"Processing {sourceImapServer} folders:");
            using (var imapSource = new ImapClient())
            using (var imapTarget = new ImapClient())
            {
                imapSource.ServerCertificateValidationCallback = (s, c, h, e) => true;
                imapTarget.ServerCertificateValidationCallback = (s, c, h, e) => true;


                imapSource.Connect(sourceImapServer, 993, true); 
                imapSource.Authenticate(sourceUsername,sourcePassword);

                imapTarget.Connect(targetImapServer, 993, true); 
                imapTarget.Authenticate(targetUsername, targetPassword);

                var sourceRootFolder = imapSource.GetFolder(imapSource.PersonalNamespaces[0]);
                var sourceFolders = sourceRootFolder.GetSubfolders();
                var targetRootFolder = imapTarget.GetFolder(imapTarget.PersonalNamespaces[0]);
                var targetFolders = targetRootFolder.GetSubfolders();
                
                foreach (var folder in sourceFolders)
                {
                    Console.Write($"{folder.Name}: ");
                    if (!targetFolders.Select(x=>x.Name).Contains(folder.Name, StringComparer.InvariantCultureIgnoreCase))
                    {
                        targetRootFolder.Create(folder.Name, true);
                        Console.Write($"Folder {folder} created. ");
                    }
                    folder.Open(MailKit.FolderAccess.ReadOnly);
                    Console.WriteLine("Total messages: {0}", folder.Count);
                    Console.WriteLine("Recent messages: {0}", folder.Recent);

                    for (int i = 0; i < folder.Count; i++)
                    {
                        var message = folder.GetMessage(i);
                        Console.WriteLine("Subject: {0}", message.Subject);
                    }
                    /*
                    Console.WriteLine($"{uids.Count} messages.");
                    var item = 0;
                    // TODO: Test if a file already exists in the target folder:
                    foreach (var msgUid in uids)
                    {
                        //var mi = imapSource.GetMessageInfoByUID(msgUid);
                        Console.Write($"{++item}/{uids.Count} [{msgUid}]");
                        var eml = imapSource.GetMessageByUID(msgUid);
                        Console.Write($" {eml.Length} bytes downloaded, uploading...\r");
                        var umi = new UploadMessageInfo { InternalDate = msgDate };
                        imapTarget.UploadMessage(folder.Name,eml, umi);
                    }
                    */
                    Console.WriteLine();
                }
                
                imapSource.Disconnect(true);
                imapTarget.Disconnect(true);
            }
            Console.ReadKey();
        }
    }
}
