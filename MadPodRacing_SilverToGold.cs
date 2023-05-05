using System;
using System.Linq;
using System.IO;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using System.Timers;
using System.Diagnostics;

/**
 * Auto-generated code below aims at helping you parse
 * the standard input according to the problem statement.
 **/
class Player
{
    //Returns the distance between two points
    private static double PointDistance(int x, int y, int lx, int ly)
    {
        return Math.Sqrt((x-lx)*(x-lx)+(y-ly)*(y-ly));
    }

    //Return the vector in a tuple object
    private static (double,double) Vector(int x1, int y1, int x2, int y2)
    {
        int vX = x2 - x1;
        int vY = y2 - y1;
        double m = Math.Sqrt(Math.Pow(vX,2)+Math.Pow(vX,2));
        return (vX/m,vY/m);
    }

    //Return the angle between two vectors
    private static double VectorAngle((double,double) v1, (double,double) v2)
    {
        var m1 = Math.Sqrt(Math.Pow(v1.Item1,2) + Math.Pow(v1.Item2,2));
        var m2 = Math.Sqrt(Math.Pow(v2.Item1,2) + Math.Pow(v2.Item2,2));

        var vv = v1.Item1*v2.Item1 + v1.Item2*v2.Item2;
        var cos = vv/(m1*m2);

        return (180 / Math.PI) * Math.Acos(cos);
    }

    static void Main(string[] args)
    {
        //Standard inputs
        string[] inputs;

        //Custom variables
        List<(int,int)> checkpoints = new List<(int, int)>();
        int torque = 0;
        bool boost = false;
        int shield = 0;
        int lastX =0, lastY = 0;
        int lastOpX =0, lastOpY = 0;
        int laps = 0;
        bool countLaps = true;
        int lastDistance = 0;
        double velocityCP;
        double velocity;
        double velocityOpp;
        (double,double) vectorCP;

        //Record time and instant time
        TimeSpan time = new TimeSpan();
        Stopwatch timer = new Stopwatch();
        Stopwatch loopTimer = new Stopwatch();
        timer.Start();
        loopTimer.Start();

        // game loop
        while (true)
        {
            Console.Error.WriteLine($"Time: {timer.Elapsed}");

            //Standard inputs
            inputs = Console.ReadLine().Split(' ');
            int x = int.Parse(inputs[0]);
            int y = int.Parse(inputs[1]);
            int nextCheckpointX = int.Parse(inputs[2]); // x position of the next check point
            int nextCheckpointY = int.Parse(inputs[3]); // y position of the next check point
            int nextCheckpointDist = int.Parse(inputs[4]); // distance to the next checkpoint
            int nextCheckpointAngle = int.Parse(inputs[5]); // angle between your pod orientation and the direction of the next checkpoint
            inputs = Console.ReadLine().Split(' ');
            int opponentX = int.Parse(inputs[0]);
            int opponentY = int.Parse(inputs[1]);

            //Calculate velocity terms
            time = loopTimer.Elapsed;
            loopTimer.Restart();
            vectorCP = Vector(nextCheckpointX,nextCheckpointY,x,y);
            
            velocityCP = (double)(lastDistance - nextCheckpointDist)/(double)time.Milliseconds;
            velocity = PointDistance(x,y,lastX,lastY)/(double)time.Milliseconds;
            velocityOpp = PointDistance(opponentX,opponentY,lastOpX,lastOpY)/(double)time.Milliseconds;
            
            //Save metrics to use later
            lastDistance = nextCheckpointDist;
            lastX = x;
            lastY = y;
            lastOpX = opponentX;
            lastOpY = opponentY;

            //Add checkpoints to use later for movement optimizations
            if(!checkpoints.Contains((nextCheckpointX,nextCheckpointY)))
            {
                checkpoints.Add((nextCheckpointX,nextCheckpointY));
            }

            //Set torque based on the angle to the checkpoint (Adaptable)
            if(Math.Abs(nextCheckpointAngle) < 15)
            {
                torque = 100;
            }
            else if(Math.Abs(nextCheckpointAngle) < 45)
            {
                torque = 100;
            }
            else if(Math.Abs(nextCheckpointAngle) < 80)
            {
                torque = 50;
            }
            else
            {
                torque = 0;
            }

            //Calculate vector and angle between checkpoints
            var index = checkpoints.IndexOf((nextCheckpointX,nextCheckpointY));
            var next = index+1 >= checkpoints.Count() ? 0 : index+1;
            var vecToNext = Vector(checkpoints[next].Item1,checkpoints[next].Item2,checkpoints[index].Item1,checkpoints[index].Item2);
            var angleToNext = VectorAngle(vectorCP, vecToNext);
            
            //Change torque if close to next checkpoint, two states based on lap count
            if(laps > 0 && velocity*50 > nextCheckpointDist && Math.Abs(angleToNext) > 80)
            {
                if(laps != 2 || next != 0)
                {
                    torque = 0;
                }
            }
            else if(laps == 0 && velocity*50 > nextCheckpointDist)
            {
                torque = 25;
            }
            
            //Lap counter 
            if(countLaps && checkpoints.Count() > 1 && index == 0)
            {
                laps++;
                countLaps = false;
            }
            else if(index != 0)
            {
                countLaps = true;
            }

            //Look at other next checkpoint on landing if all checkpoints are recorded
            if(velocity*50 > nextCheckpointDist && laps > 0)
            {
                if(laps != 2 || next != 0)
                {
                    nextCheckpointX = checkpoints[next].Item1;
                    nextCheckpointY = checkpoints[next].Item2;
                }
            }

            //Metrics
            Console.Error.WriteLine($"Shield: {shield}");
            Console.Error.WriteLine($"Boost: {boost}");
            Console.Error.WriteLine($"Checkpoint Distance: {nextCheckpointDist}");
            Console.Error.WriteLine($"Checkpoint Angle: {nextCheckpointAngle}");
            Console.Error.WriteLine($"Velocity to Checkpoint: {String.Format("{0:0.000}", velocityCP)}");
            Console.Error.WriteLine($"Velocity: {String.Format("{0:0.000}", velocity)}");
            Console.Error.WriteLine($"Velocity Opponent: {String.Format("{0:0.000}", velocityOpp)}");
            Console.Error.WriteLine($"Torque: {torque}");
            Console.Error.WriteLine($"Laps: {laps}");
            Console.Error.Write($"Checkpoints:");
            checkpoints.ForEach(x => Console.Error.Write($" ({x.Item1},{x.Item2}) |"));
            Console.Error.Write($"\n");

            //Opponent variables
            double opponentDistance = PointDistance(x,y,opponentX,opponentY);
            var vecToOpponent = Vector(opponentX,opponentY,x,y);
            var angleToOpPosition = VectorAngle(vectorCP, vecToOpponent);
            //Opponent metrics
            Console.Error.WriteLine($"Distance between pods: {String.Format("{0:0.000}", opponentDistance)}");
            Console.Error.WriteLine($"Angle between pods: {String.Format("{0:0.000}", angleToOpPosition)}");

            //Output
            if(opponentDistance < 900 && angleToOpPosition > 90)
            {
                //First usage of shield based on opponent positon and angle
                Console.WriteLine(nextCheckpointX + " " + nextCheckpointY + " SHIELD");
                shield++;
            }
            else if(laps > 0 && nextCheckpointDist > 4000 && velocityCP > 30 && opponentDistance > 2000 && Math.Abs(nextCheckpointAngle) < 5 && !boost)
            {
                //Boost conditions
                Console.WriteLine(nextCheckpointX + " " + nextCheckpointY + " BOOST");
                boost = true;
            }
            else
            {
                //Use torque to the nextCheckpoint
                Console.WriteLine(nextCheckpointX + " " + nextCheckpointY + " " + torque);
            }
        }
    }
}