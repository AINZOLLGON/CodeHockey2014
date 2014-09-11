using System;
using Com.CodeGame.CodeHockey2014.DevKit.CSharpCgdk.Model;

namespace Com.CodeGame.CodeHockey2014.DevKit.CSharpCgdk
{
    public sealed class MyStrategy : IStrategy
    {
        private static double STRIKE_ANGLE = 1.0D * Math.PI / 180.0D;
        public void Move(Hockeyist self, World world, Game game, Move move)
        {
            if (self.State == HockeyistState.Swinging)
            {
                move.Action = ActionType.Strike;
                return;
            }

            if (world.Puck.OwnerPlayerId == self.PlayerId)
            {
                if (world.Puck.OwnerHockeyistId == self.Id)
                {
                    Player opponentPlayer = world.GetOpponentPlayer();

                    double netX = 0.5D * (opponentPlayer.NetBack + opponentPlayer.NetFront);
                    double netY = 0.5D * (opponentPlayer.NetBottom + opponentPlayer.NetTop);
                    netY += (self.Y < netY ? 0.5D : -0.5D) * game.GoalNetHeight;

                    double angleToNet = self.GetAngleTo(netX, netY);
                    move.Turn = angleToNet;

                    if (Math.Abs(angleToNet) < STRIKE_ANGLE)
                    {
                        move.Action = ActionType.Swing;
                    }
                }
                else
                {
                    Hockeyist nearestOpponent = getNearestOpponent(self.X, self.Y, world);
                    if (nearestOpponent != null)
                    {
                        if (self.GetDistanceTo(nearestOpponent) > game.StickLength)
                        {
                            move.SpeedUp = 1.0D;
                        }
                        else if (Math.Abs(self.GetAngleTo(nearestOpponent)) < 0.5D * game.StickSector)
                        {
                            move.Action = ActionType.Strike;
                        }
                        move.Turn = self.GetAngleTo(nearestOpponent);
                    }
                }
            }
            else
            {
                move.SpeedUp = 1.0D;
                move.Turn = self.GetAngleTo(world.Puck);
                move.Action = ActionType.TakePuck;
            }
        }

        private static Hockeyist getNearestOpponent(double x, double y, World world) {
        Hockeyist nearestOpponent = null;
        double nearestOpponentRange = 0.0D;

        foreach (Hockeyist hockeyist in world.Hockeyists) {
            if (hockeyist.IsTeammate || hockeyist.Type == HockeyistType.Goalie
                    || hockeyist.State == HockeyistState.KnockedDown
                    || hockeyist.State == HockeyistState.Resting) {
                continue;
            }

            double opponentRange = hypot(x - hockeyist.X, y - hockeyist.Y);

            if (nearestOpponent == null || opponentRange < nearestOpponentRange) {
                nearestOpponent = hockeyist;
                nearestOpponentRange = opponentRange;
            }
        }

        return nearestOpponent;
    }

        private static double hypot(double a, double b)
        {
            return Math.Sqrt(Math.Pow(a, 2) + Math.Pow(b, 2));
        }
    }
}