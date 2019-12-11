using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Deltin.Deltinteger.Parse;
using OmniSharp.Extensions.LanguageServer.Protocol.Server;
using OmniSharp.Extensions.LanguageServer.Protocol.Models;
using OmniSharp.Extensions.LanguageServer.Protocol.Client.Capabilities;
using OmniSharp.Extensions.LanguageServer.Protocol.Server.Capabilities;

namespace Deltin.Deltinteger.LanguageServer
{
    class CompletionHandler : ICompletionHandler
    {
        private DeltintegerLanguageServer _languageServer { get; }

        public CompletionHandler(DeltintegerLanguageServer languageServer)
        {
            _languageServer = languageServer;
        }

        public async Task<CompletionList> Handle(CompletionParams completionParams, CancellationToken token)
        {
            List<CompletionItem> items = new List<CompletionItem>();

            // Get default type completion.
            foreach (var defaultType in CodeType.DefaultTypes)
                items.Add(defaultType.GetCompletion());

            // If the script has not been parsed yet, return the default completion.
            if (_languageServer.LastParse == null) return items;

            // Add the user defined types.
            foreach (var definedType in _languageServer.LastParse.definedTypes)
                items.Add(definedType.GetCompletion());

            // Get the script from the uri. If it isn't parsed, return the default completion. 
            var script = _languageServer.LastParse.ScriptFromUri(completionParams.TextDocument.Uri);
            if (script == null) return items;

            var completions = script.GetCompletionRanges();
            List<CompletionRange> inRange = new List<CompletionRange>();
            foreach (var completion in completions)
                if (completion.Range.IsInside(completionParams.Position))
                    inRange.Add(completion);
            
            if (inRange.Count > 0)
            {
                inRange = inRange
                    // Order by if the completion range has priority. True is first.
                    .OrderByDescending(range => range.Priority)
                    // Then order by the size of the ranges.
                    .ThenBy(range => range.Range)
                    .ToList();
                
                if (inRange[0].Priority)
                {
                    items.Clear();
                    inRange.RemoveRange(1, inRange.Count - 1);
                }
                
                foreach (var range in inRange)
                    items.AddRange(range.Scope.GetCompletion(completionParams.Position));
            }
            return items;
        }

        public CompletionRegistrationOptions GetRegistrationOptions()
        {
            return new CompletionRegistrationOptions()
            {
                DocumentSelector = DocumentHandler._documentSelector,
                // Most tools trigger completion request automatically without explicitly requesting
                // it using a keyboard shortcut (e.g. Ctrl+Space). Typically they do so when the user
                // starts to type an identifier. For example if the user types `c` in a JavaScript file
                // code complete will automatically pop up present `console` besides others as a
                // completion item. Characters that make up identifiers don't need to be listed here.
                //
                // If code complete should automatically be trigger on characters not being valid inside
                // an identifier (for example `.` in JavaScript) list them in `triggerCharacters`.
                TriggerCharacters = new Container<string>("."),
                // The server provides support to resolve additional
                // information for a completion item.
                ResolveProvider = false
            };
        }

        // Client compatibility
        private CompletionCapability _capability;
        public void SetCapability(CompletionCapability capability)
        {
            _capability = capability;
        }
    }
}