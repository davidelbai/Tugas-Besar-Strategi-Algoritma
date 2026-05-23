using System;
using System.Drawing;
using Robocode.TankRoyale.BotApi;
using Robocode.TankRoyale.BotApi.Events;

public class SniperBot : Bot
{
    private double enemyX = -1;
    private double enemyY = -1;
    private double enemyEnergy = 100;
    private int scanMissCount = 0;

    private const double OPTIMAL_MIN_DIST = 250;
    private const double OPTIMAL_MAX_DIST = 500;

    static void Main(string[] args)
    {
        new SniperBot().Start();
    }

    public SniperBot() : base(BotInfo.FromFile("SniperBot.json")) { }

    public override void Run()
    {
        BodyColor = Color.Cyan;
        TurretColor = Color.Green;
        RadarColor = Color.Red;
        BulletColor = Color.Black;
        
        while (IsRunning)
        {
            scanMissCount++;

            if (scanMissCount > 5)
            {
                TurnRadarRight(45);
                TurnRight(15);      
                Forward(40);
            }
            else
            {
                TurnRadarRight(20);
            }
        }
    }

    public override void OnScannedBot(ScannedBotEvent evt)
    {
        scanMissCount = 0; 

        enemyX = evt.X;
        enemyY = evt.Y;
        double dist = DistanceTo(evt.X, evt.Y);
        double prevEnemyEnergy = enemyEnergy;
        enemyEnergy = evt.Energy;

        double bestFP = 0.1;
        double bestGain = double.MinValue;

        double[] fpOptions = { 0.5, 1.0, 1.5, 2.0, 2.5, 3.0 };
        foreach (double fp in fpOptions)
        {

            if (Energy <= fp) continue;

            double bulletSpeed = 20 - (3 * fp);
            double timeToHit = dist / bulletSpeed;

            double hitChance = Math.Max(0.1, 1.0 - (timeToHit * 0.05));

            double gain = (3 * fp * hitChance) - fp;
            if (gain > bestGain)
            {
                bestGain = gain;
                bestFP = fp;
            }
        }

        double bulletSpeed2 = 20 - (3 * bestFP);
        double timeToHit2 = dist / bulletSpeed2;
        double predictedX = evt.X + Math.Sin(evt.Direction * Math.PI / 180) * evt.Speed * timeToHit2;
        double predictedY = evt.Y + Math.Cos(evt.Direction * Math.PI / 180) * evt.Speed * timeToHit2;

        TurnGunLeft(GunBearingTo(predictedX, predictedY));

        if (bestGain > 0)
        {
            Fire(bestFP);
        }

        if (dist < OPTIMAL_MIN_DIST)
        {

            TurnLeft(BearingTo(evt.X, evt.Y) + 180); // balik arah
            Forward(OPTIMAL_MIN_DIST - dist);
        }
        else if (dist > OPTIMAL_MAX_DIST)
        {

            TurnLeft(BearingTo(evt.X, evt.Y));
            Forward(Math.Min(dist - OPTIMAL_MAX_DIST, 80));
        }
        else
        {

            TurnRight(90);
            Forward(60);
        }

        TurnRadarLeft(RadarBearingTo(evt.X, evt.Y));
    }

    public override void OnHitByBullet(HitByBulletEvent evt)
    {

        double bearing = CalcBearing(evt.Bullet.Direction);
        TurnLeft(90 - bearing);
        Forward(100);
        TurnRight(30);
    }

    public override void OnHitWall(HitWallEvent evt)
    {
        Back(40);
        TurnRight(60);
    }

    public override void OnHitBot(HitBotEvent evt)
    {
        
        Back(60);
        TurnRight(45);
    }
}
