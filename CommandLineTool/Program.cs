using System;
using System.Diagnostics;
using System.Linq;
using Cielo24;
using Cielo24.Options;
using CommandLine;

namespace CommandLineTool
{
    class Program
    {
        static readonly Options Options = new Options();
        static readonly Parser OptionParser = new Parser();
        static readonly Actions Actions = new Actions();
        static string invokedVerb;

        static void Main(string[] args)
        {
            //string[] argss = { "./prog.exe", "-a", "create", "-u", "testscript", "-p", "testscript2", "-m", "https://www.youtube.com/watch?v=n1JGzxvsRPg" };
            if (args.Length == 1 && Options.Verbs.Contains(args[0]))
            {
                Options.PrintActionHelp(args[0]);
            }
            else if (args.Length != 0 && Options.Verbs.Contains(args[0])) // If verb is valid
            {
                invokedVerb = args[0];
                if (OptionParser.ParseArguments(args, Options)) // If parsing successful
                {
                    if (Options.VerboseMode) // Enable verbose mode
                    {
                        Debug.Listeners.Add(new TextWriterTraceListener(Console.Out));
                    }

                    if (invokedVerb.Equals("login") || invokedVerb.Equals("logout")) // Login and logout are special cases
                    {
                        CallAction(invokedVerb);
                    }
                    else if (TryLogin())                                             // All other actions
                    {
                        CallAction(invokedVerb);
                    }
                }
                else // Parsing failed: show usage for verb
                {
                    Options.PrintActionHelp(invokedVerb);
                }
            }
            else
            {
                Options.PrintDefaultUsage();
            }
        }

        public static void CallAction(string actionName)
        {
            Actions.ServerUrl = Options.ServerUrl;

            switch (actionName)
            {
                // ACCESS CONTROL //
                case "login":
                    Console.WriteLine("Logging in...");
                    if (Options.ApiSecureKey.Equals(Guid.Empty))
                    { // Use password and username
                        TryAction(() => Actions.Login(Options.Username, Options.Password, Options.HeaderLogin));
                    }
                    else
                    { // Use secure key
                        TryAction(() => Actions.Login(Options.Username, Options.ApiSecureKey, Options.HeaderLogin));
                    }
                    break;
                case "logout":
                    Console.WriteLine("Logging out...");
                    TryAction(() =>
                    {
                        Actions.Logout(Options.ApiToken);
                        return "Logged out successfully";
                    });
                    break;
                case "generate_api_key":
                    Console.WriteLine("Generating API key...");
                    TryAction(() => Actions.GenerateAPIKey(Options.ApiToken, Options.Username, Options.ForceNew));
                    break;
                case "remove_api_key":
                    Console.WriteLine("Removing API key...");
                    TryAction(() =>
                    {
                        Actions.RemoveAPIKey(Options.ApiToken, Options.ApiSecureKey);
                        return "API Key removed successfully";
                    });
                    break;
                case "update_password":
                    Console.WriteLine("Updating password...");
                    TryAction(() =>
                    {
                        Actions.UpdatePassword(Options.ApiToken, Options.NewPassword);
                        return "Password updated successfully";
                    });
                    break;
                // JOB CONTROL //
                case "create":
                    TryAction(() =>
                    {
                        Console.WriteLine("Creating job...");
                        var jobId = Actions.CreateJob(Options.ApiToken, Options.JobName, Options.SourceLanguage).JobId;
                        Console.WriteLine("JobId: " + jobId);
                        Console.WriteLine("Adding media...");
                        if (Options.MediaFile == null)
                        {
                            Console.WriteLine("TaskId: " +
                                              Actions.AddMediaToJob(Options.ApiToken, jobId, Options.MediaUrl)
                                                  .ToString("N"));
                        }
                        else
                        {
                            Console.WriteLine("TaskId: " +
                                              Actions.AddMediaToJob(Options.ApiToken, jobId, Options.MediaFile)
                                                  .ToString("N"));
                        }
                        Console.WriteLine("Performing transcrition...");
                        var pto = new PerformTranscriptionOptions();
                        pto.PopulateFromArray(Options.JobConfig);
                        return Actions.PerformTranscription(Options.ApiToken, jobId, Options.Fidelity, Options.Priority,
                            Options.CallbackUrl, Options.TurnaroundHours, Options.TargetLanguage, pto);
                    });
                    break;
                case "authorize":
                    Console.WriteLine("Authorizing job...");
                    TryAction(() =>
                    {
                        Actions.AuthorizeJob(Options.ApiToken, Options.JobId);
                        return "Job Authorized Succesfully";
                    });
                    break;
                case "delete":
                    Console.WriteLine("Deleting job...");
                    TryAction(() => Actions.DeleteJob(Options.ApiToken, Options.JobId));
                    break;
                case "job_info":
                    Console.WriteLine("Getting job info...");
                    TryAction(() => Actions.GetJobInfo(Options.ApiToken, Options.JobId));
                    break;
                case "list":
                    Console.WriteLine("Listing jobs...");
                    TryAction(() => Actions.GetJobList(Options.ApiToken));
                    break;
                case "add_media_to_job":
                    Console.WriteLine("Ading media to job...");
                    if (Options.MediaUrl != null)
                    { // Media Url
                        TryAction(() => Actions.AddMediaToJob(Options.ApiToken, Options.JobId, Options.MediaUrl));
                    }
                    else
                    { // Media File
                        TryAction(() => Actions.AddMediaToJob(Options.ApiToken, Options.JobId, Options.MediaFile));
                    }
                    break;
                case "add_embedded_media_to_job":
                    Console.WriteLine("Adding embedded media to job...");
                    TryAction(() => Actions.AddEmbeddedMediaToJob(Options.ApiToken, Options.JobId, Options.MediaUrl));
                    break;
                case "get_media":
                    Console.WriteLine("Getting media...");
                    TryAction(() => Actions.GetMedia(Options.ApiToken, Options.JobId));
                    break;
                case "get_transcript":
                    Console.WriteLine("Getting transcript...");
                    var to = new TranscriptOptions();
                    to.PopulateFromArray(Options.CaptionOptions);
                    TryAction(() => Actions.GetTranscript(Options.ApiToken, Options.JobId, to));
                    break;
                case "get_caption":
                    Console.WriteLine("Getting caption...");
                    var co = new CaptionOptions();
                    co.PopulateFromArray(Options.CaptionOptions);
                    TryAction(() => Actions.GetCaption(Options.ApiToken, Options.JobId, Options.CaptionFormat, co));
                    break;
                case "get_elementlist":
                    Console.WriteLine("Getting element list...");
                    TryAction(() => Actions.GetElementList(Options.ApiToken, Options.JobId, Options.ElementlistVersion));
                    break;
                case "list_elementlists":
                    Console.WriteLine("Listing element lists...");
                    TryAction(() => string.Join("\n", Actions.GetListOfElementLists(Options.ApiToken, Options.JobId)));
                    break;
                default:
                    Options.PrintDefaultUsage();
                    break;
            }
        }

        private static void TryAction(Func<object> action)
        {
            try
            {
                var output = action.Invoke().ToString();
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("\n" + output);
                Console.ResetColor();
            }
            catch (Exception e)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("\n" + e.Message);
                Console.ResetColor();

                Options.PrintActionHelp(invokedVerb);
            }
        }

        private static bool TryLogin()
        {
            Actions.ServerUrl = Options.ServerUrl;
            if (!Options.ApiToken.Equals(Guid.Empty))
                return true;
            try
            {
                Options.ApiToken = Options.Password != null 
                    ? Actions.Login(Options.Username, Options.Password, true) 
                    : Actions.Login(Options.Username, Options.ApiSecureKey, true);
            }
            catch (Exception e)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("\n" + e.Message);
                Console.ResetColor();
                Options.PrintActionHelp(invokedVerb);
                return false;
            }
            return true;
        }
    }
}