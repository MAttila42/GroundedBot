using System;
using System.Collections.Generic;
using System.Timers;
using Microsoft.Scripting;
using Microsoft.Scripting.Hosting;
using CodingSeb.ExpressionEvaluator;
using IronPython.Hosting;
using Discord;
using GroundedBot.Json;

namespace GroundedBot.Commands
{
    class Evaluate
    {
        public static List<ulong> AllowedRoles =
            new List<ulong>(BaseConfig.GetConfig().Roles.Admin);

        public static string[] Aliases =
        {
            "evaluate",
            "eval"
        };

        static IUserMessage response;
        static string result;
        static int counter;
        static Timer timer;

        public async static void DoCommand()
        {
            await Program.Log("command");

            var message = Recieved.Message;
            string[] m = message.Content.Split();
            response = await message.Channel.SendMessageAsync("Evaluating...", allowedMentions: AllowedMentions.None);
            string code;
            try { code = message.Content.Substring(m[0].Length + m[1].Length + 2, message.Content.Length - m[0].Length - m[1].Length - 2); }
            catch (Exception)
            {
                await response.ModifyAsync(m => m.Content = "❌ Add code to evaluate!");
                return;
            }
            result = null;
            counter = 0;
            try
            {
                timer = new Timer(1000);
                timer.Elapsed += CheckIfTimedOut;
                timer.AutoReset = true;
                timer.Enabled = true;
                switch (m[1])
                {
                    case "cs":
                        //result = Z.Expressions.Eval.Execute(code).ToString();
                        ExpressionEvaluator csEvaluator = new ExpressionEvaluator();
                        result = csEvaluator.ScriptEvaluate(code).ToString();
                        break;
                    case "py":
                        var py = new PythonScript();
                        result = py.RunFromString<string>(code, "output");
                        break;
                    case "js":
                        result = ScriptEngineJs.Eval("jscript", code).ToString();
                        break;

                    default:
                        await response.ModifyAsync(m => m.Content = "❌ Unkown language!");
                        timer.Enabled = false;
                        timer.Stop();
                        timer.Dispose();
                        return;
                }
            }
            catch (Exception e) { result = e.Message; }
            if (result.Length <= 2000)
                await response.ModifyAsync(m => m.Content = $"```{result}```");
            else
                await response.ModifyAsync(m => m.Content = "❌ 2000+ characters!");
            timer.Enabled = false;
            timer.Stop();
            timer.Dispose();
        }

        static void CheckIfTimedOut(object source, ElapsedEventArgs e)
        {
            if (counter++ > 30 && result == null)
            {
                response.ModifyAsync(m => m.Content = "❌ Timed out!");
                timer.Enabled = false;
                timer.Stop();
                timer.Dispose();
            }
        }
    }

    public class PythonScript
    {
        private ScriptEngine _engine;
        public PythonScript() { _engine = Python.CreateEngine(); }
        public TResult RunFromString<TResult>(string code, string variableName)
        {
            ScriptSource source = _engine.CreateScriptSourceFromString(code, SourceCodeKind.Statements);
            CompiledCode cc = source.Compile();
            ScriptScope scope = _engine.CreateScope();
            cc.Execute(scope);
            return scope.GetVariable<TResult>(variableName);
        }
    }
}
