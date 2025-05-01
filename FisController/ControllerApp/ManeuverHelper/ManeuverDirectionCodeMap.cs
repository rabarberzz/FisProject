using ControllerApp.Resources;

namespace ControllerApp.ManeuverHelper
{
    public static class ManeuverDirectionCodeMap
    {
        private static readonly Dictionary<(string ManeuverType, string ManeuverModifier), string> directionCodeMap = new()
        {
            { (ManeuverTypes.Turn, ManeuverModifiers.Left), DirectionsCodes.LeftTurn },
            { (ManeuverTypes.Turn, ManeuverModifiers.Right), DirectionsCodes.RightTurn },
            { (ManeuverTypes.Turn, ManeuverModifiers.SlightRight), DirectionsCodes.SlightRight },
            { (ManeuverTypes.Turn, ManeuverModifiers.SlightLeft), DirectionsCodes.SlightLeft },
            { (ManeuverTypes.Turn, ManeuverModifiers.SharpRight), DirectionsCodes.RightTurn },
            { (ManeuverTypes.Turn, ManeuverModifiers.SharpLeft), DirectionsCodes.LeftTurn},
            { (ManeuverTypes.Turn, ManeuverModifiers.Uturn ), DirectionsCodes.LeftTurnaround },

            { (ManeuverTypes.EndOfRoad, ManeuverModifiers.Left), DirectionsCodes.LeftTurn },
            { (ManeuverTypes.EndOfRoad, ManeuverModifiers.Right), DirectionsCodes.RightTurn },
            { (ManeuverTypes.EndOfRoad, ManeuverModifiers.SlightRight), DirectionsCodes.SlightRight },
            { (ManeuverTypes.EndOfRoad, ManeuverModifiers.SlightLeft), DirectionsCodes.SlightLeft },
            { (ManeuverTypes.EndOfRoad, ManeuverModifiers.SharpRight), DirectionsCodes.RightTurn },
            { (ManeuverTypes.EndOfRoad, ManeuverModifiers.SharpLeft), DirectionsCodes.LeftTurn},

            { (ManeuverTypes.OnRamp, ManeuverModifiers.SharpRight), DirectionsCodes.RightTurn },
            { (ManeuverTypes.OnRamp, ManeuverModifiers.Right), DirectionsCodes.RightTurn },
            { (ManeuverTypes.OnRamp, ManeuverModifiers.SlightRight), DirectionsCodes.SlightRight },
            { (ManeuverTypes.OnRamp, ManeuverModifiers.SlightLeft), DirectionsCodes.SlightLeft },
            { (ManeuverTypes.OnRamp, ManeuverModifiers.Left), DirectionsCodes.LeftTurn },
            { (ManeuverTypes.OnRamp, ManeuverModifiers.SharpLeft), DirectionsCodes.LeftTurn },
            { (ManeuverTypes.OnRamp, ManeuverModifiers.Straight), DirectionsCodes.Straight },

            { (ManeuverTypes.OffRamp, ManeuverModifiers.SharpRight), DirectionsCodes.RightTurn },
            { (ManeuverTypes.OffRamp, ManeuverModifiers.Right), DirectionsCodes.RightTurn },
            { (ManeuverTypes.OffRamp, ManeuverModifiers.SlightRight), DirectionsCodes.SlightRight },
            { (ManeuverTypes.OffRamp, ManeuverModifiers.SlightLeft), DirectionsCodes.SlightLeft },
            { (ManeuverTypes.OffRamp, ManeuverModifiers.Left), DirectionsCodes.LeftTurn },
            { (ManeuverTypes.OffRamp, ManeuverModifiers.SharpLeft), DirectionsCodes.LeftTurn },
            { (ManeuverTypes.OffRamp, ManeuverModifiers.Straight), DirectionsCodes.Straight },

            { (ManeuverTypes.Fork, ManeuverModifiers.Left), DirectionsCodes.LeftTurn },
            { (ManeuverTypes.Fork, ManeuverModifiers.SlightLeft), DirectionsCodes.SlightLeft },
            { (ManeuverTypes.Fork, ManeuverModifiers.SharpLeft), DirectionsCodes.LeftTurn },

            { (ManeuverTypes.Fork, ManeuverModifiers.Straight), DirectionsCodes.Straight },

            { (ManeuverTypes.Fork, ManeuverModifiers.Right), DirectionsCodes.RightTurn },
            { (ManeuverTypes.Fork, ManeuverModifiers.SlightRight), DirectionsCodes.SlightRight },
            { (ManeuverTypes.Fork, ManeuverModifiers.SharpRight), DirectionsCodes.RightTurn },

            { (ManeuverTypes.Merge, ManeuverModifiers.Straight), DirectionsCodes.Straight },

            { (ManeuverTypes.Merge, ManeuverModifiers.SlightRight), DirectionsCodes.SlightRight },
            { (ManeuverTypes.Merge, ManeuverModifiers.Right), DirectionsCodes.RightTurn },
            { (ManeuverTypes.Merge, ManeuverModifiers.SharpRight), DirectionsCodes.RightTurn },

            { (ManeuverTypes.Merge, ManeuverModifiers.SlightLeft), DirectionsCodes.SlightLeft },
            { (ManeuverTypes.Merge, ManeuverModifiers.Left), DirectionsCodes.LeftTurn },
            { (ManeuverTypes.Merge, ManeuverModifiers.SharpLeft), DirectionsCodes.LeftTurn },

            { (ManeuverTypes.Continue, ManeuverModifiers.Straight), DirectionsCodes.Straight },
            { (ManeuverTypes.Continue, ManeuverModifiers.Uturn), DirectionsCodes.LeftTurnaround },

            { (ManeuverTypes.Continue, ManeuverModifiers.Left), DirectionsCodes.LeftTurn },
            { (ManeuverTypes.Continue, ManeuverModifiers.SlightLeft), DirectionsCodes.SlightLeft },
            { (ManeuverTypes.Continue, ManeuverModifiers.SharpLeft), DirectionsCodes.LeftTurn },
            
            { (ManeuverTypes.Continue, ManeuverModifiers.Right), DirectionsCodes.RightTurn},
            { (ManeuverTypes.Continue, ManeuverModifiers.SlightRight), DirectionsCodes.SlightRight },
            { (ManeuverTypes.Continue, ManeuverModifiers.SharpRight), DirectionsCodes.RightTurn},

            { (ManeuverTypes.RoundaboutTurn, ManeuverModifiers.Straight), DirectionsCodes.Straight },

            { (ManeuverTypes.RoundaboutTurn, ManeuverModifiers.Left), DirectionsCodes.LeftTurn },
            { (ManeuverTypes.RoundaboutTurn, ManeuverModifiers.SlightLeft), DirectionsCodes.SlightLeft },
            { (ManeuverTypes.RoundaboutTurn, ManeuverModifiers.SharpLeft), DirectionsCodes.LeftTurn },

            { (ManeuverTypes.RoundaboutTurn, ManeuverModifiers.Right), DirectionsCodes.RightTurn},
            { (ManeuverTypes.RoundaboutTurn, ManeuverModifiers.SlightRight), DirectionsCodes.SlightRight },
            { (ManeuverTypes.RoundaboutTurn, ManeuverModifiers.SharpRight), DirectionsCodes.RightTurn},

        };

        public static string? GetDirectionCode(string maneuverType, string maneuverModifier)
        {
            if (maneuverType == ManeuverTypes.Roundabout ||
                maneuverType == ManeuverTypes.Rotary ||
                maneuverType == ManeuverTypes.ExitRoundabout ||
                maneuverType == ManeuverTypes.ExitRotary)
            {
                return DirectionsCodes.Roundabout;
            }

            if (maneuverType == ManeuverTypes.Depart || maneuverType == ManeuverTypes.Arrive)
            {
                return DirectionsCodes.Straight;
            }

            if (directionCodeMap.TryGetValue((maneuverType, maneuverModifier), out var directionCode))
            {
                return directionCode;
            }
            return null;
        }
    }
}
