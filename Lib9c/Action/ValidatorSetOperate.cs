using System;
using System.Numerics;
using Bencodex.Types;
using Libplanet.Action;
using Libplanet.Action.State;
using Libplanet.Crypto;
using Libplanet.Types.Consensus;
using Nekoyume.Module;

namespace Nekoyume.Action
{
    [Serializable]
    [ActionType(TypeIdentifier)]
    public sealed class ValidatorSetOperate : ActionBase, IEquatable<ValidatorSetOperate>
    {
        public const string TypeIdentifier = "op_validator_set";

        // fixme: this is a temporary key for backward compatibility.
        private static readonly byte[] PowerKey = { 0x70 }; // 'p'
        private static readonly byte[] PublicKeyKey = { 0x50 }; // 'P'

        public ValidatorSetOperate()
        {
            Error = "An uninitialized action.";
        }

        public ValidatorSetOperate(ValidatorSetOperatorType @operator, Validator operand)
        {
            Error = null;
            Operator = @operator;
            Operand = operand;
        }

        public string Error { get; private set; }

        public ValidatorSetOperatorType Operator { get; private set; }

        public Validator Operand { get; private set; }

        public static ValidatorSetOperate Append(Validator operand) =>
            new ValidatorSetOperate(ValidatorSetOperatorType.Append, operand);

        public static ValidatorSetOperate Remove(Validator operand) =>
            new ValidatorSetOperate(ValidatorSetOperatorType.Remove, operand);

        public static ValidatorSetOperate Update(Validator operand) =>
            new ValidatorSetOperate(ValidatorSetOperatorType.Update, operand);

        public static readonly string ValidatorSetOperateKey = "vsok";

        public bool Equals(ValidatorSetOperate other)
        {
            if (ReferenceEquals(this, other))
            {
                return true;
            }

            return Error == other.Error &&
                   Operator == other.Operator &&
                   Operand.Equals(other.Operand);
        }

        public override bool Equals(object obj)
        {
            return ReferenceEquals(this, obj) ||
                   (obj is ValidatorSetOperate other && Equals(other));
        }

        public override int GetHashCode() =>
            (Error, (int)Operator, Operand).GetHashCode();

        public override IWorld Execute(IActionContext context)
        {
            GasTracer.UseGas(1);
            if (Error != null)
            {
                throw new InvalidOperationException(Error);
            }

            CheckPermission(context);

            IWorld previousState = context.PreviousState;
            ValidatorSet validatorSet = previousState.GetValidatorSet();

            Func<ValidatorSet, Validator, Validator> func = Operator.ToFunc();
            return previousState.SetValidatorSet(previousState
                .GetValidatorSet()
                .Update(func(validatorSet, Operand)));
        }

        public override IValue PlainValue =>
            Dictionary.Empty
                .Add("type_id", TypeIdentifier)
                .Add("values", (Error is null)
                    ? (IValue)Bencodex.Types.Dictionary.Empty
                        .Add(
                            "op",
                            Operator is ValidatorSetOperatorType op
                                ? new Text(op.ToString())
                                : (IValue)Null.Value
                        )
                        .Add("operand", Operand.Bencoded)
                    : (Text)Error);

        public override void LoadPlainValue(IValue plainValue)
        {
            IValue values = ((Dictionary)plainValue)["values"];

            if (values is Text t)
            {
                Error = t;
                return;
            }

            if (!(values is Dictionary d))
            {
                Error =
                    "The action serialization is invalid; " +
                    "the serialization should be a dictionary.";
                return;
            }

            if (!d.TryGetValue((Text)"op", out IValue opValue))
            {
                Error = "The serialized dictionary lacks the key \"op\".";
                return;
            }

            if (!(opValue is Text opText))
            {
                Error = "The serialized \"op\" field is not a text.";
                return;
            }

            string opStr = opText.Value;
            if (!Enum.TryParse(opStr, true, out ValidatorSetOperatorType op))
            {
                Error = $"The serialized operator \"{opStr}\" is invalid.";
                return;
            }

            if (!d.TryGetValue((Text)"operand", out IValue operandValue))
            {
                Error = "The serialized dictionary lacks the key \"operand\".";
                return;
            }

            if (!(operandValue is Bencodex.Types.Dictionary operandDict))
            {
                Error = "The serialized \"operand\" field is not an dictionary.";
                return;
            }

            Operator = op;
            // FIXME: This is a temporary code for backward compatibility.
            Operand = BackwardCompatibility(operandDict);
            Error = null;
        }

        private Validator BackwardCompatibility(Bencodex.Types.Dictionary dict)
        {
            try
            {
                return new Validator(dict);
            }
            catch (Exception)
            {
                BigInteger power =
                    new BigInteger(((Binary)dict[PowerKey]).ToByteArray());
                PublicKey publicKey =
                    new PublicKey(((Binary)dict[PublicKeyKey]).ToByteArray());

                return new Validator(publicKey, power);
            }
        }
    }
}
