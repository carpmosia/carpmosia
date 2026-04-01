using Lidgren.Network;
using Robust.Shared.Network;
using Robust.Shared.Prototypes; // Carpmosia-edit - Better map vote
using Robust.Shared.Serialization;

namespace Content.Shared.Voting
{
    public sealed class MsgVoteData : NetMessage
    {
        public override MsgGroups MsgGroup => MsgGroups.Command;

        public int VoteId;
        public bool VoteActive;
        public string VoteTitle = string.Empty;
        public string VoteInitiator = string.Empty;
        public TimeSpan StartTime; // Server RealTime.
        public TimeSpan EndTime; // Server RealTime.
        public (ushort votes, string name, string? icon, EntProtoId? preview)[] Options = default!; // Carpmosia-edit - Better map vote
        public bool IsYourVoteDirty;
        public byte? YourVote;
        public bool DisplayVotes;
        public int TargetEntity;

        public override void ReadFromBuffer(NetIncomingMessage buffer, IRobustSerializer serializer)
        {
            VoteId = buffer.ReadVariableInt32();
            VoteActive = buffer.ReadBoolean();
            buffer.ReadPadBits();

            if (!VoteActive)
                return;

            VoteTitle = buffer.ReadString();
            VoteInitiator = buffer.ReadString();
            StartTime = TimeSpan.FromTicks(buffer.ReadInt64());
            EndTime = TimeSpan.FromTicks(buffer.ReadInt64());
            DisplayVotes = buffer.ReadBoolean();
            TargetEntity = buffer.ReadVariableInt32();

            Options = new (ushort votes, string name, string? icon, EntProtoId? preview)[buffer.ReadByte()]; // Carpmosia-edit - Better map vote
            for (var i = 0; i < Options.Length; i++)
            {
                Options[i] = (buffer.ReadUInt16(), buffer.ReadString(), buffer.ReadBoolean() ? buffer.ReadString() : null, buffer.ReadBoolean() ? buffer.ReadString() : null); // Carpmosia-edit - Better map vote
            }

            IsYourVoteDirty = buffer.ReadBoolean();
            if (IsYourVoteDirty)
            {
                YourVote = buffer.ReadBoolean() ? buffer.ReadByte() : null;
            }
        }

        public override void WriteToBuffer(NetOutgoingMessage buffer, IRobustSerializer serializer)
        {
            buffer.WriteVariableInt32(VoteId);
            buffer.Write(VoteActive);
            buffer.WritePadBits();

            if (!VoteActive)
                return;

            buffer.Write(VoteTitle);
            buffer.Write(VoteInitiator);
            buffer.Write(StartTime.Ticks);
            buffer.Write(EndTime.Ticks);
            buffer.Write(DisplayVotes);
            buffer.WriteVariableInt32(TargetEntity);

            buffer.Write((byte) Options.Length);
            foreach (var (votes, name, icon, preview) in Options) // Carpmosia-edit - Better map vote
            {
                buffer.Write(votes);
                buffer.Write(name);
                // Carpmosia-edit - Better map vote
                buffer.Write(icon != null);
                if (icon != null)
                {
                  buffer.Write(icon);
                }
                buffer.Write(preview.HasValue);
                if (preview.HasValue)
                {
                  buffer.Write(preview.Value);
                }
                // Carpmosia-edit - Better map vote
            }

            buffer.Write(IsYourVoteDirty);
            if (IsYourVoteDirty)
            {
                buffer.Write(YourVote.HasValue);
                if (YourVote.HasValue)
                {
                    buffer.Write(YourVote.Value);
                }
            }
        }

        public override NetDeliveryMethod DeliveryMethod => NetDeliveryMethod.ReliableOrdered;
    }
}
