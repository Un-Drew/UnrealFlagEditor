using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using System.Xml.Serialization;

namespace UnrealFlagEditor
{
    public class EditorHeadless
    {
        static public XmlReader CurrentReader = null;
        static public List<HeadlessMessage> HeadlessMessages = null;

        static public int PerformHeadlessXmlFile(string packageFilePath, string instructionsFilePath)
        {
            int ret = 1;

            List<HeadlessMessage> prevHeadlessMessages = EditorHeadless.HeadlessMessages;
            EditorHeadless.HeadlessMessages = new List<HeadlessMessage>();

            RegisterConsoleMessage($"Running headless instructions from {instructionsFilePath} on package {packageFilePath}", HeadlessMessage.MessageType.Info);

            HeadlessInstructions instr = LoadXMLInstructionFile(instructionsFilePath);

            EditorEngine edEngine = null;
            if (instr != null)
            {
                if (packageFilePath != null)
                {
                    try
                    {
                        edEngine = new EditorEngine(EditorEngine.EditorLoadFlags.Headless);
                        try
                        {
                            edEngine.LoadPackage(packageFilePath);
                        }
                        catch (Exception e)
                        {
                            string error = EditorEngine.GetUserFriendlyFileError(packageFilePath, "package", e);
                            if (error != null)
                                RegisterConsoleError(error);
                            else
                                throw;
                        }

                        if (edEngine.LoadedPackage != null)
                        {
                            ret = ExecuteXMLInstructions(edEngine, instr);
                        }
                    }
                    catch (Exception e)
                    {
                        RegisterConsoleError($"Unexpected exception while running instructions for package {packageFilePath}: {e}");
                    }

                    if (edEngine != null)
                    {
                        edEngine.Dispose();  // Also disposes the loaded package.
                        edEngine = null;
                    }
                }
                else
                {
                    RegisterConsoleError("Package file path may not be null!");
                }
            }

            SortAndDisplayHeadlessMessages();

            EditorHeadless.HeadlessMessages = prevHeadlessMessages;

            return ret;
        }

        static public HeadlessInstructions LoadXMLInstructionFile(string xmlFilePath)
        {
            try
            {
                using (FileStream file = File.OpenRead(xmlFilePath))
                {
                    XmlReader prevReader = EditorHeadless.CurrentReader;

                    XmlSerializer serializer = new XmlSerializer(typeof(HeadlessInstructions));

                    EditorHeadless.CurrentReader = new XmlTextReader(file);

                    XmlElementEventHandler unknownElement = ((sender, e) =>
                    {
                        RegisterConsoleError(e.LineNumber, $"Unknown element {e.Element.Name}!");
                    });

                    XmlAttributeEventHandler unknownAttribute = ((sender, e) =>
                    {
                        RegisterConsoleError(e.LineNumber, $"Unknown attribute {e.Attr.Name}!");
                    });

                    serializer.UnknownElement += unknownElement;
                    serializer.UnknownAttribute += unknownAttribute;

                    HeadlessInstructions instr = null;
                    try
                    {
                        instr = (HeadlessInstructions)serializer.Deserialize(EditorHeadless.CurrentReader);
                    }
                    catch (InvalidOperationException e)
                    {
                        if (e.InnerException != null)
                        {
                            RegisterConsoleError(
                                (EditorHeadless.CurrentReader is IXmlLineInfo) ? ((IXmlLineInfo)EditorHeadless.CurrentReader).LineNumber : -1,
                                $"Error while reading XML file: {e.InnerException.Message}"
                            );
                        }
                        else
                        {
                            RegisterConsoleError(
                                (EditorHeadless.CurrentReader is IXmlLineInfo) ? ((IXmlLineInfo)EditorHeadless.CurrentReader).LineNumber : -1,
                                $"Unexpected exception while reading XML file: {e.ToString()}"
                            );
                        }
                    }

                    EditorHeadless.CurrentReader = prevReader;

                    serializer.UnknownElement -= unknownElement;
                    serializer.UnknownAttribute -= unknownAttribute;

                    return instr;
                }
            }
            catch (Exception e)
            {
                string error = EditorEngine.GetUserFriendlyFileError(xmlFilePath, "XML file", e);
                if (error != null)
                    RegisterConsoleError(error);
                else
                    RegisterConsoleError($"Unexpected exception trying to open the XML file {xmlFilePath}: {e}");
            }

            return null;
        }

        static public int ExecuteXMLInstructions(EditorEngine edEngine, HeadlessInstructions instr)
        {
            instr.ApplyProperties(edEngine);

            if (instr.ObjectsNotFound > 0 || instr.PropertiesNotFound > 0)
            {
                RegisterConsoleMessage(
                    $"{instr.ObjectsNotFound} Objects and {instr.PropertiesNotFound} Properties were not found, and skipped.",
                    HeadlessMessage.MessageType.Warning
                );
            }

            int errorCount = 0, warningCount = 0;
            foreach (var message in EditorHeadless.HeadlessMessages)
            {
                switch (message.MT)
                {
                    case HeadlessMessage.MessageType.Warning:
                        warningCount++;
                        break;
                    case HeadlessMessage.MessageType.Error:
                        errorCount++;
                        break;
                }
            }

            edEngine.ConditionalUpdateStats();

            uint changeCount = edEngine.CachedChangeCount;

            bool saveAborted = (changeCount == 0 || errorCount > 0);

            bool saveSuccessful;
            if (!saveAborted)
            {
                saveSuccessful = true;
                try
                {
                    edEngine.SaveOverwrite();
                }
                catch (Exception e)
                {
                    RegisterConsoleError($"Unexpected exception while saving package: {e}");
                    saveSuccessful = false;
                    errorCount++;
                }
            }
            else
                saveSuccessful = false;

            uint propsAlreadyAtDesiredValue = instr.AttemptedChanges - changeCount;
            if (errorCount <= 0)
            {
                if (!instr.HasAnyProperties)
                {
                    RegisterConsoleMessage("Couldn't find any property tags in this XML file.", HeadlessMessage.MessageType.Warning);
                    warningCount++;
                }
                else if (propsAlreadyAtDesiredValue > 0 && !instr.PrintEachChange)
                {
                    RegisterConsoleMessage(
                        ((propsAlreadyAtDesiredValue == 1) ? "One property was" : $"{propsAlreadyAtDesiredValue} properties were")
                        + " already at the desired value, and " + (propsAlreadyAtDesiredValue == 1 ? "was" : "were") + " left unchanged.",
                        HeadlessMessage.MessageType.Notification
                    );
                }
            }

            RegisterConsoleMessage(
                (saveAborted ? "Save aborted" : saveSuccessful ? "Flag edits successful" : "Save failed/incomplete")
                + $" for package '{edEngine?.LoadedPackage?.PackageName}'"
                + ((errorCount <= 0 && changeCount == 0) ? " (due to a lack of changes)" : "")
                + $" - Changes: {changeCount}, Errors: {errorCount}, Warnings: {warningCount}.",
                (errorCount > 0) ? HeadlessMessage.MessageType.Error :
                (warningCount > 0) ? HeadlessMessage.MessageType.Warning :
                (instr.AttemptedChanges > 0) ? HeadlessMessage.MessageType.Success :
                HeadlessMessage.MessageType.Info,
                supressTypeText: true
            );

            return (errorCount <= 0 && (saveSuccessful || saveAborted)) ? 0 : 1;
        }

        static public void SortAndDisplayHeadlessMessages()
        {
            EditorHeadless.HeadlessMessages.Sort((x, y) =>
            {
                // Lineless messages should act like sort separators. I... think that's how it works, anyway.
                if (x.LineNumber == -1 || y.LineNumber == -1)
                    return 0;

                return x.LineNumber - y.LineNumber;
            });

            HeadlessMessage.MessageType lastMessageType = HeadlessMessage.MessageType.Info;
            foreach (var message in EditorHeadless.HeadlessMessages)
            {
                if (lastMessageType != message.MT)
                {
                    lastMessageType = message.MT;
                    HeadlessMessage.SetConsoleColor(message.MT);
                }
                Program.WriteConsoleMessage(message.ToString());
            }
            Console.ResetColor();
        }

        static public void RegisterConsoleError(int lineNumber, HeadlessException e)
        {
            RegisterConsoleMessage(lineNumber, e.Message, HeadlessMessage.MessageType.Error);
        }

        static public void RegisterConsoleError(string message)
        {
            RegisterConsoleMessage(-1, message, HeadlessMessage.MessageType.Error);
        }

        static public void RegisterConsoleError(int lineNumber, string message)
        {
            RegisterConsoleMessage(lineNumber, message, HeadlessMessage.MessageType.Error);
        }

        static public void RegisterConsoleMessage(string message, HeadlessMessage.MessageType mt, bool supressTypeText = false)
        {
            RegisterConsoleMessage(-1, message, mt, supressTypeText: supressTypeText);
        }

        static public void RegisterConsoleMessage(int lineNumber, string message, HeadlessMessage.MessageType mt, bool supressTypeText = false)
        {
            if (EditorHeadless.HeadlessMessages == null) return;
            EditorHeadless.HeadlessMessages.Add(new HeadlessMessage(lineNumber, message, mt, supressTypeText));
        }
    }

    public struct HeadlessMessage
    {
        public enum MessageType
        {
            Info,
            Warning,
            Error,
            Success,
            Notification
        };

        public int LineNumber;
        public string Message;
        public MessageType MT;
        public bool SupressTypeText;

        public HeadlessMessage(int lineNumber, string message, MessageType mt = MessageType.Info, bool supressTypeText = false)
        {
            LineNumber = lineNumber;
            Message = message;
            MT = mt;
            SupressTypeText = supressTypeText;
        }

        public override string ToString()
        {
            string str = null;
            switch (MT)
            {
                case MessageType.Warning:
                    str = "WARNING";
                    break;
                case MessageType.Error:
                    str = "ERROR";
                    break;
            }

            if (LineNumber > -1)
            {
                if (str == null)
                    str = $"(line {LineNumber}): ";
                else
                    str = $"{str} (line {LineNumber}): ";
            }
            else if (str != null)
                str += ": ";

            if (str == null)
                return Message;
            else
                return str + Message;
        }

        static public void SetConsoleColor(MessageType mt)
        {
            switch (mt)
            {
                case MessageType.Info:
                    Console.ResetColor();
                    break;
                case MessageType.Warning:
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    break;
                case MessageType.Error:
                    Console.ForegroundColor = ConsoleColor.Red;
                    break;
                case MessageType.Success:
                    Console.ForegroundColor = ConsoleColor.Green;
                    break;
                case MessageType.Notification:
                    Console.ForegroundColor = ConsoleColor.Cyan;
                    break;
            }
        }
    }
}
