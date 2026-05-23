using System;
using System.Drawing;
using Robocode.TankRoyale.BotApi;
using Robocode.TankRoyale.BotApi.Events;

public class Ngaboot : Bot
{
    private double enemyX = -1;
    private double enemyY = -1;
    private double enemyDistance = double.MaxValue;

    static void Main(string[] args)
    {
        new Ngaboot().Start();
    }

    public Ngaboot() : base(BotInfo.FromFile("Ngaboot.json")) { }

    public override void Run()
    {
        BodyColor = Color.Cyan;
        TurretColor = Color.Red;
        RadarColor = Color.Pink;
        BulletColor = Color.Green;
        
        while (IsRunning)
        {

            TurnRadarRight(45);

            if (enemyX < 0)
            {
                Forward(50);
                TurnRight(30);
            }
        }
    }

    public override void OnScannedBot(ScannedBotEvent evt)
    {

        enemyX = evt.X;
        enemyY = evt.Y;
        enemyDistance = DistanceTo(evt.X, evt.Y);

        double firePower;
        if (Energy > 50)
            firePower = 3.0; 
        else if (Energy > 20)
            firePower = 1.5; 
        else
            firePower = 0.5; 

        double angleToEnemy = BearingTo(evt.X, evt.Y);
        TurnGunLeft(GunBearingTo(evt.X, evt.Y));

        Fire(firePower);

        if (enemyDistance > 200)
        {
            TurnLeft(BearingTo(evt.X, evt.Y));
            Forward(Math.Min(enemyDistance / 2, 100));
        }
        else
        {
            TurnRight(90);
            Forward(50);
        }
    }
    public override void OnHitByBullet(HitByBulletEvent evt)
    {
        double bearing = CalcBearing(evt.Bullet.Direction);
        TurnLeft(90 - Math.Abs(bearing));
        Forward(75);
    }
    public override void OnHitWall(HitWallEvent evt)
    {
        Back(30);
        TurnRight(45);
    }
    public override void OnHitBot(HitBotEvent evt)
    {
        
        Forward(30);
    }
}
