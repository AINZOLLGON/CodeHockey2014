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
                    
                    Random rand = new Random();
                    Hockeyist nearestFriendly = getNearestFriendly(self.X, self.Y, world);
                    if ((nearestFriendly != null) && (nearestFriendly.State == HockeyistState.Active)
                        && (rand.Next(0,1) == 1))
                    {
                        move.Turn = self.GetAngleTo(nearestFriendly);
                        move.PassPower = 0.6;
                        move.Action = ActionType.Pass;
                    }
                    else
                    {
                        Player opponentPlayer = world.GetOpponentPlayer();
                        if (isGoalPosition(self.X, self.Y, world) || (isGameWithoutGoalie(game, world)))
                        {
                            move.SpeedUp = -0.5D;
                          
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
                            move.SpeedUp = 1.0D;
                            if (opponentPlayer.NetFront < (world.Width / 2))//���� ������ ���������� �� ����� ������� ����
                            {
                                if (self.Y > (game.WorldHeight/2))//���� ������� �� ������ �������� ����
                                {
                                    move.Turn = self.GetAngleTo(300, 576);
                                }
                                else//����� ������� �� ������� �������� ����
                                {
                                    move.Turn = self.GetAngleTo(300, 317);
                                }
                            }
                            else//����� ������ ������� ����
                            {
                                if (self.Y > (game.WorldHeight / 2))//���� ������� �� ������ �������� ����
                                {
                                    move.Turn = self.GetAngleTo(724, 576);
                                }
                                else//����� ������� �� ������� �������� ����
                                {
                                    move.Turn = self.GetAngleTo(724, 317);
                                }
                            }
                        }
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
                //if (world.Puck.)
                {
                    move.SpeedUp = 1.0D;
                    move.Turn = self.GetAngleTo(world.Puck);
                    move.Action = ActionType.TakePuck;
                }
            }
        }

        private static bool isGameWithoutGoalie(Game game, World world)//���� ��� ��������?
        {
            if ((game.TickCount > 6000) && //���� ������ �������� �����
                ((world.GetMyPlayer().GoalCount + world.GetOpponentPlayer().GoalCount) == 0)) //� ���� ���� 0:0
                return true;
            else
                return false;
        }

        private static bool isGoalPosition(double x, double y, World world)
        {
            Player opponentPlayer = world.GetOpponentPlayer();
            if (opponentPlayer.NetFront < (world.Width / 2))//���� ������ ���������� �� ����� ������� ����
            {
                if ((x >= 262) && (x <= 400) && (y >= 554) && (y <= 720))
                    return true;
                else
                    if ((x >= 262) && (x <= 400) && (y >= 173) && (y <= 339))
                        return true;
                    else
                        return false;
            }
            else//����� ������ ������� ����
            {
                if ((x >= 624) && (x <= 762) && (y >= 554) && (y <= 720))
                    return true;
                else
                    if ((x >= 624) && (x <= 762) && (y >= 173) && (y <= 339))
                        return true;
                    else
                        return false;
            }
            
        }

        private static Hockeyist getNearestOpponent(double x, double y, World world) 
        {
            Hockeyist nearestOpponent = null;
            double nearestOpponentRange = 0.0D;

            foreach (Hockeyist hockeyist in world.Hockeyists) 
            {
                if (hockeyist.IsTeammate || hockeyist.Type == HockeyistType.Goalie
                        || hockeyist.State == HockeyistState.KnockedDown
                        || hockeyist.State == HockeyistState.Resting) 
                {
                    continue;
                }

                double opponentRange = hypot(x - hockeyist.X, y - hockeyist.Y);

                if (nearestOpponent == null || opponentRange < nearestOpponentRange) 
                {
                    nearestOpponent = hockeyist;
                    nearestOpponentRange = opponentRange;
                }
            }

            return nearestOpponent;
        }

        private static Hockeyist getNearestFriendly(double x, double y, World world)
        {
            Hockeyist nearestFriendly = null;
            //double nearestFriendlyRange = 0.0D;

            foreach (Hockeyist hockeyist in world.Hockeyists)
            {
                if ((hockeyist.Id != world.Puck.OwnerHockeyistId) && (hockeyist.IsTeammate) 
                    && (hockeyist.Type == HockeyistType.Versatile) && (hockeyist.State == HockeyistState.Active))
                {
                    //double opponentRange = hypot(x - hockeyist.X, y - hockeyist.Y);

                    //if (nearestFriendly == null || opponentRange < nearestFriendlyRange)
                    //{
                        nearestFriendly = hockeyist;
                      //  nearestFriendlyRange = opponentRange;
                    //}
                }  
            }

            return nearestFriendly;
        }

        private static double hypot(double a, double b)
        {
            return Math.Sqrt(Math.Pow(a, 2) + Math.Pow(b, 2));
        }
    }
}