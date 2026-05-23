using System;
using System.Drawing;
using System.Collections.Generic;
using Robocode.TankRoyale.BotApi;
using Robocode.TankRoyale.BotApi.Events;

public class SurvivorBot : Bot
{
    private double enemyX = -1;
    private double enemyY = -1;
    private double enemyEnergy = 100;

    private bool isEvading = false;
    private int evadeTurns = 0;
    private double moveDirection = 1; 
    private static readonly Random rng = new Random();

    private const double DANGER_DISTANCE = 200; 
    private const double SAFE_DISTANCE = 400;   // jarak aman

    static void Main(string[] args)
    {
        new SurvivorBot().Start();
    }

    public SurvivorBot() : base(BotInfo.FromFile("SurvivorBot.json")) { }

    public override void Run()
    {
        BodyColor = Color.White;
        TurretColor = Color.Pink;
        RadarColor = Color.Purple;
        BulletColor = Color.Red;

        while (IsRunning)
        {
            if (isEvading && evadeTurns > 0)
            {
                evadeTurns--;
                Forward(moveDirection * 30);
            }
            else
            {
                isEvading = false;

                PerformSurvivalMovement();
            }

            TurnRadarRight(45);
        }
    }

    private void PerformSurvivalMovement()
    {

        if (enemyX >= 0)
        {
            double distToEnemy = DistanceTo(enemyX, enemyY);

            if (distToEnemy < DANGER_DISTANCE)
            {

                double fleeAngle = BearingTo(enemyX, enemyY) + 180;
                TurnLeft(fleeAngle);
                Forward(80);
            }
            else
            {

                TurnRight(10 * moveDirection);
                Forward(50);
            }
        }
        else
        {
            TurnRight(15);
            Forward(50);
        }
    }

    public override void OnScannedBot(ScannedBotEvent evt)
    {

        double prevEnergy = enemyEnergy;
        enemyX = evt.X;
        enemyY = evt.Y;
        enemyEnergy = evt.Energy;
        double dist = DistanceTo(evt.X, evt.Y);

        double energyDrop = prevEnergy - evt.Energy;
        bool enemyJustFired = (energyDrop > 0 && energyDrop <= 3.0);

        if (enemyJustFired)
        {
            isEvading = true;
            evadeTurns = 5;

            double evadeAngle = BearingTo(evt.X, evt.Y) + 90;
            TurnLeft(evadeAngle);
            moveDirection = (RandomBoolean() ? 1 : -1); 
            Forward(moveDirection * 100);
        }

        if (dist < 300 && Energy > 30)
        {

            double fp = (dist < 150) ? 1.0 : 0.5;

            TurnGunLeft(GunBearingTo(evt.X, evt.Y));
            Fire(fp);
        }

        if (dist < DANGER_DISTANCE)
        {

            isEvading = true;
            evadeTurns = 8;
            TurnLeft(BearingTo(evt.X, evt.Y) + 160); 
            Forward(120);
        }
    }

    private bool RandomBoolean()
    {
        return rng.Next(2) == 0;
    }

    public override void OnHitByBullet(HitByBulletEvent evt)
    {

        isEvading = true;
        evadeTurns = 6;
        double bearing = CalcBearing(evt.Bullet.Direction);
        TurnLeft(90 - bearing);
        moveDirection = (bearing > 0) ? 1 : -1;
        Forward(moveDirection * 100);
    }

    public override void OnHitWall(HitWallEvent evt)
    {
        // Jauhi dinding
        Back(30);
        TurnRight(60);
        isEvading = false;
    }

    public override void OnHitBot(HitBotEvent evt)
    {

        Back(50);
        TurnLeft(45);
        isEvading = false;
    }

    public override void OnBotDeath(BotDeathEvent evt)
    {

    }
}
