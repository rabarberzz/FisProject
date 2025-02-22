using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ControllerApp.Resources
{
    public static class DirectionsCodes
    {
        public const string LeftTurn =
            "\x30\x33\x33\x7A" +
            "\x3E\x41\x3A\x7A" +
            "\x74\x74\x3A";

        public const string Roundabout =
            "\x11\x12\x13\x7A" +
            "\x1D\x1E\x1F\x20\x21\x7A" +
            "\x2A\x2B\x2C\x2D\x2E\x7A" +
            "\x39\x3A\x3B\x7A\x41";

        public const string RightTurn =
            "\x33\x33\x36\x7A" +
            "\x3A\x41\x44\x7A" +
            "\x3A\x74\x74";

        public const string Straight =
            "\x0B\x0C\x0D\x7A" +
            "\x74\x3A\x74\x7A" +
            "\x74\x3A\x74\x7A";

        public const string LeftTurnaround =
            "\x74\x33\x33\x33\x7A" +
            "\x74\x3A\x41\x3A\x7A" +
            "\x61\x62\x63\x3A\x7A" +
            "\x74\x74\x74\x3A";

        public const string RightTurnaround =
            "\x33\x33\x33\x74\x7A" +
            "\x3A\x41\x3A\x74\x7A" +
            "\x3A\x61\x62\x63\x7A" +
            "\x3A\x74\x74\x74";

        public const string LeftCutoff =
            "\x09\x0A\x74\x09\x7A" +
            "\x3A\x24\x74\x3A\x7A" +
            "\x01\x23\x25\x3A\x7A" +
            "\x74\x4D\x23\x3A\x7A" +
            "\x74\x74\x4D\x3A";

        public const string RightCutoff =
            "\x09\x74\x0E\x09\x7A" +
            "\x3A\x74\x27\x3A\x7A" +
            "\x3A\x26\x28\x02\x7A" +
            "\x3A\x28\x4E\x74\x7A" +
            "\x3A\x4E\x74\x74";

        public const string SlightRight =
            "\x74\x0E\x09\x7A" +
            "\x74\x27\x3A\x7A" +
            "\x26\x28\x02\x7A" +
            "\x3A\x4E\x74";

        public const string SlightLeft =
            "\x09\x0A\x74\x7A" +
            "\x3A\x24\x74\x7A" +
            "\x01\x23\x25\x7A" +
            "\x74\x4D\x3A";

        public static Dictionary<string, string> GetDirectionsCodes()
        {
            return new Dictionary<string, string>
            {
                { "LeftTurn", LeftTurn },
                { "Roundabout", Roundabout },
                { "RightTurn", RightTurn },
                { "Straight", Straight },
                { "LeftTurnaround", LeftTurnaround },
                { "RightTurnaround", RightTurnaround },
                { "LeftCutoff", LeftCutoff },
                { "RightCutoff", RightCutoff },
                { "SlightRight", SlightRight },
                { "SlightLeft", SlightLeft }
            };
        }
    }
}
