using System;
using System.Collections.Generic;
using Deltin.Deltinteger.Elements;
using Deltin.Deltinteger.Compiler;

namespace Deltin.Deltinteger.Parse
{
    public class ReturnHandler
    {
        protected readonly ActionSet ActionSet;
        private readonly bool MultiplePaths;

        // If `MultiplePaths` is true, use `ReturnStore`. Else use `ReturningValue`.
        private readonly IndexReference ReturnStore;
        private IWorkshopTree ReturningValue;

        private bool ValueWasReturned;

        private readonly List<SkipStartMarker> skips = new List<SkipStartMarker>();

        public List<RecursiveIndexReference> AdditionalPopOnReturn { get; } = new List<RecursiveIndexReference>();

        public ReturnHandler(ActionSet actionSet, string methodName, bool multiplePaths)
        {
            ActionSet = actionSet;
            MultiplePaths = multiplePaths;

            if (multiplePaths)
                ReturnStore = actionSet.VarCollection.Assign("_" + methodName + "ReturnValue", actionSet.IsGlobal, true);
        }

        public virtual void ReturnValue(IWorkshopTree value)
        {
            if (!MultiplePaths && ValueWasReturned)
                throw new Exception("_multiplePaths is set as false and 2 expressions were returned.");
            ValueWasReturned = true;

            // Multiple return paths.
            if (MultiplePaths)
                ActionSet.AddAction(ReturnStore.SetVariable((Element)value));
            // One return path.
            else
                ReturningValue = value;
        }

        public virtual void Return(Scope returningFromScope, ActionSet returningSet)
        {
            if (returningSet.IsRecursive)
            {
                returningFromScope.EndScope(returningSet, true);

                foreach (var recursiveIndexReference in AdditionalPopOnReturn)
                    returningSet.AddAction(recursiveIndexReference.Pop());
            }

            SkipStartMarker returnSkipStart = new SkipStartMarker(returningSet);
            returningSet.AddAction(returnSkipStart);
            skips.Add(returnSkipStart);
        }

        public virtual void ApplyReturnSkips()
        {
            SkipEndMarker methodEndMarker = new SkipEndMarker();
            ActionSet.AddAction(methodEndMarker);

            foreach (var returnSkip in skips)
                returnSkip.SetEndMarker(methodEndMarker);
        }

        public virtual IWorkshopTree GetReturnedValue()
        {
            if (MultiplePaths)
                return ReturnStore.GetVariable();
            else
                return ReturningValue;
        }
    }

    public class RuleReturnHandler : ReturnHandler
    {
        public RuleReturnHandler(ActionSet actionSet) : base(actionSet, null, false) {}

        public override void ApplyReturnSkips() => throw new Exception("Can't apply return skips in a rule.");
        public override IWorkshopTree GetReturnedValue() => throw new Exception("Can't get the returned value of a rule.");
        public override void ReturnValue(IWorkshopTree value) => throw new Exception("Can't return a value in a rule.");

        public override void Return(Scope returningFromScope, ActionSet returningSet)
        {
            ActionSet.AddAction(Element.Part<A_Abort>());
        }
    }

    public interface IParseReturnHandler
    {
        void Validate(DocRange range, IExpression value);
    }

    public class ParseReturnHandler : IParseReturnHandler
    {
        public CodeType ExpectedType { get; }
        public bool AllowMultiple { get; }
        public bool MustReturnValue { get; }
        private readonly ParseInfo _parseInfo;
        private bool _returnFound;
        // Errors
        public string MustReturnValueMessage { get; set; } = "Must return a value.";
        public string MoreThanOneReturnMessage { get; set; } = "Cannot have more than one return statement if the function's return type is constant.";
        public string VoidReturnValueMessage { get; set; }

        public ParseReturnHandler(ParseInfo parseInfo)
        {
            _parseInfo = parseInfo;
        }

        public ParseReturnHandler(ParseInfo parseInfo, string objectName) : this(parseInfo)
        {
            VoidReturnValueMessage = objectName + " is void, so no value can be returned.";
        }

        public void Validate(DocRange range, IExpression value)
        {
            // Error if a value must be returned.
            if (MustReturnValue && value == null)
            {
                _parseInfo.Script.Diagnostics.Error(MustReturnValueMessage, range);
                return;
            }

            // Multiple return statements when not allowed.
            if (!AllowMultiple && _returnFound)
            {
                _parseInfo.Script.Diagnostics.Error(MoreThanOneReturnMessage, range);
                return;
            }
            _returnFound = true;
        }
    }
}