using System;
using Deltin.Deltinteger.Elements;
using CompletionItem = OmniSharp.Extensions.LanguageServer.Protocol.Models.CompletionItem;
using CompletionItemKind = OmniSharp.Extensions.LanguageServer.Protocol.Models.CompletionItemKind;

namespace Deltin.Deltinteger.Parse
{
    public class PlayerType : CodeType, IAdditionalArray, IResolveElements
    {
        // These functions are shared with both Player and Player[] types.
        // * Teleport *
        FuncMethod Teleport => new FuncMethodBuilder() {
            Name = "Teleport",
            Parameters = new CodeParameter[] {
                new CodeParameter("position", "The position to teleport the player or players to. Can be a player or a vector.", Positionable.Instance)
            },
            Documentation = "Teleports one or more players to the specified location.",
            Action = (actionSet, methodCall) => {
                actionSet.AddAction(Element.Part("Teleport", actionSet.CurrentObject, methodCall.ParameterValues[0]));
                return null;
            }
        };
        // * SetMoveSpeed *
        FuncMethod SetMoveSpeed => new FuncMethodBuilder() {
            Name = "SetMoveSpeed",
            Parameters = new CodeParameter[] {
                new CodeParameter("moveSpeedPercent", "The percentage of raw move speed to which the player or players will set their move speed.", _supplier.Number())
            },
            Documentation = "Sets the move speed of one or more players.",
            Action = (actionSet, methodCall) => {
                actionSet.AddAction(Element.Part("Set Move Speed", actionSet.CurrentObject, methodCall.ParameterValues[0]));
                return null;
            }
        };
        // * SetMaxHealth *
        FuncMethod SetMaxHealth => new FuncMethodBuilder() {
            Name = "SetMaxHealth",
            Parameters = new CodeParameter[] {
                new CodeParameter("healthPercent", "The percentage of raw max health to which the player or players will set their max health.", _supplier.Number())
            },
            Documentation = "Sets the move speed of one or more players.",
            Action = (actionSet, methodCall) => {
                actionSet.AddAction(Element.Part("Set Max Health", actionSet.CurrentObject, methodCall.ParameterValues[0]));
                return null;
            }
        };

        public readonly Scope ObjectScope = new Scope("player variables") { TagPlayerVariables = true };
        private readonly ITypeSupplier _supplier;

        public PlayerType(ITypeSupplier typeSupplier) : base("Player")
        {
            CanBeExtended = false;
            Inherit(Positionable.Instance, null, null);
            Kind = "struct";
            _supplier = typeSupplier;
        }

        public void ResolveElements()
        {            
            AddSharedFunctionsToScope(ObjectScope);

            AddFunc(new FuncMethodBuilder() {
                Name = "IsButtonHeld",
                Parameters = new CodeParameter[] { new CodeParameter("button", ValueGroupType.GetEnumType("Button")) },
                ReturnType = BooleanType.Instance,
                Action = (set, call) => Element.Part("Is Button Held", set.CurrentObject, call.ParameterValues[0]),
                Documentation = "Determines if the target player is holding a button."
            });
            AddFunc(new FuncMethodBuilder() {
                Name = "IsCommunicating",
                Parameters = new CodeParameter[] { new CodeParameter("communication", ValueGroupType.GetEnumType("Communication")) },
                ReturnType = BooleanType.Instance,
                Action = (set, call) => Element.Part("Is Communicating", set.CurrentObject, call.ParameterValues[0]),
                Documentation = "Determines if the target player is communicating."
            });
            AddFunc("Position", _supplier.Vector(), (set, call) => Element.PositionOf(set.CurrentObject), "The position of the player.");
            AddFunc("Team", _supplier.Any(), (set, call) => Element.Part("Team Of", set.CurrentObject), "The team of the player.");
            AddFunc("Health", _supplier.Number(), (set, call) => Element.Part("Health", set.CurrentObject), "The health of the player.");
            AddFunc("MaxHealth", _supplier.Number(), (set, call) => Element.Part("Max Health", set.CurrentObject), "The maximum health of the player.");
            AddFunc("FacingDirection", _supplier.Vector(), (set, call) => Element.FacingDirectionOf(set.CurrentObject), "The facing direction of the player.");
            AddFunc("Hero", _supplier.Any(), (set, call) => Element.Part("Hero Of", set.CurrentObject), "The hero of the player.");
            AddFunc("IsHost", _supplier.Boolean(), (set, call) => Element.Compare(set.CurrentObject, Operator.Equal, Element.Part("Host Player")), "Determines if the player is the host.");
            AddFunc("IsAlive", _supplier.Boolean(), (set, call) => Element.Part("Is Alive", set.CurrentObject), "Determines if the player is alive.");
            AddFunc("IsDead", _supplier.Boolean(), (set, call) => Element.Part("Is Dead", set.CurrentObject), "Determines if the player is dead.");
            AddFunc("IsCrouching", _supplier.Boolean(), (set, call) => Element.Part("Is Crouching", set.CurrentObject), "Determines if the player is crouching.");
            AddFunc("IsDummy", _supplier.Boolean(), (set, call) => Element.Part("Is Dummy Bot", set.CurrentObject), "Determines if the player is a dummy bot.");
            AddFunc("IsFiringPrimary", _supplier.Boolean(), (set, call) => Element.Part("Is Firing Primary", set.CurrentObject), "Determines if the player is firing their primary weapon.");
            AddFunc("IsFiringSecondary", _supplier.Boolean(), (set, call) => Element.Part("Is Firing Secondary", set.CurrentObject), "Determines if the player is using their secondary attack.");
            AddFunc("IsInAir", _supplier.Boolean(), (set, call) => Element.Part("Is In Air", set.CurrentObject), "Determines if the player is in the air.");
            AddFunc("IsOnGround", _supplier.Boolean(), (set, call) => Element.Part("Is On Ground", set.CurrentObject), "Determines if the player is on the ground.");
            AddFunc("IsInSpawnRoom", _supplier.Boolean(), (set, call) => Element.Part("Is In Spawn Room", set.CurrentObject), "Determines if the player is in the spawn room.");
            AddFunc("IsMoving", _supplier.Boolean(), (set, call) => Element.Part("Is Moving", set.CurrentObject), "Determines if the player is moving.");
            AddFunc("IsOnObjective", _supplier.Boolean(), (set, call) => Element.Part("Is On Objective", set.CurrentObject), "Determines if the player is on the objective.");
            AddFunc("IsOnWall", _supplier.Boolean(), (set, call) => Element.Part("Is On Wall", set.CurrentObject), "Determines if the player is on a wall.");
            AddFunc("IsPortraitOnFire", _supplier.Boolean(), (set, call) => Element.Part("Is Portrait On Fire", set.CurrentObject), "Determines if the player's portrait is on fire.");
            AddFunc("IsStanding", _supplier.Boolean(), (set, call) => Element.Part("Is Standing", set.CurrentObject), "Determines if the player is standing.");
            AddFunc("IsUsingAbility1", _supplier.Boolean(), (set, call) => Element.Part("Is Using Ability 1", set.CurrentObject), "Determines if the player is using their ability 1.");
            AddFunc("IsUsingAbility2", _supplier.Boolean(), (set, call) => Element.Part("Is Using Ability 2", set.CurrentObject), "Determines if the player is using their ability 2.");
        }

        public override CompletionItem GetCompletion() => new CompletionItem() {
            Label = Name,
            Kind = CompletionItemKind.Struct
        };
        public override Scope GetObjectScope() => ObjectScope;
        public override Scope ReturningScope() => null;
        private void AddFunc(FuncMethodBuilder builder) => ObjectScope.AddNativeMethod(new FuncMethod(builder));
        private void AddFunc(string name, CodeType returnType, Func<ActionSet, MethodCall, IWorkshopTree> action, string documentation) => AddFunc(new FuncMethodBuilder() { Name = name, ReturnType = returnType, Action = action, Documentation = documentation });
        public void OverrideArray(ArrayType array)
        {
            AddSharedFunctionsToScope(array.Scope);
            array.Scope.TagPlayerVariables = true;
        }
        void AddSharedFunctionsToScope(Scope scope)
        {
            scope.AddNativeMethod(Teleport);
            scope.AddNativeMethod(SetMoveSpeed);
            scope.AddNativeMethod(SetMaxHealth);
        }
    }
}