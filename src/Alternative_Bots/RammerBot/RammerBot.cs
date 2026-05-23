using System;
using System.Drawing;
using System.Collections.Generic;
using Robocode.TankRoyale.BotApi;
using Robocode.TankRoyale.BotApi.Events;

public class RammerBot : Bot
{

    private int targetId = -1;
    private double targetX = -1;
    private double targetY = -1;
    private double targetEnergy = double.MaxValue;

    private Dictionary<int, (double x, double y, double energy)> knownEnemies
        = new Dictionary<int, (double, double, double)>();

    static void Main(string[] args)
    {
        new RammerBot().Start();
    }

    public RammerBot() : base(BotInfo.FromFile("RammerBot.json")) { }

    public override void Run()
    {
        BodyColor = Color.Cyan;
        TurretColor = Color.Yellow;
        RadarColor = Color.Pink;
        BulletColor = Color.Blue;

        while (IsRunning)
        {

            TurnRadarRight(45);

            if (targetX >= 0)
            {
                ChaseTarget();
            }
            else
            {

                TurnRight(20);
                Forward(80);
            }
        }
    }

    private void ChaseTarget()
    {
        double distToTarget = DistanceTo(targetX, targetY);

        double bearingToTarget = BearingTo(targetX, targetY);
        TurnLeft(bearingToTarget);

        if (distToTarget > 20)
        {
            Forward(distToTarget);
        }

        TurnGunLeft(GunBearingTo(targetX, targetY));
        if (GunHeat == 0 && Energy > 10)
        {

            Fire(0.5);
        }
    }

    public override void OnScannedBot(ScannedBotEvent evt)
    {

        knownEnemies[evt.ScannedBotId] = (evt.X, evt.Y, evt.Energy);

        SelectWeakestTarget();

        if (targetId == evt.ScannedBotId)
        {
            TurnRadarLeft(RadarBearingTo(evt.X, evt.Y));
        }
    }

    private void SelectWeakestTarget()
    {
        int bestId = -1;
        double lowestEnergy = double.MaxValue;
        double bestX = -1, bestY = -1;

        foreach (var kvp in knownEnemies)
        {
            if (kvp.Value.energy < lowestEnergy)
            {
                lowestEnergy = kvp.Value.energy;
                bestId = kvp.Key;
                bestX = kvp.Value.x;
                bestY = kvp.Value.y;
            }
        }

        if (bestId >= 0)
        {
            targetId = bestId;
            targetX = bestX;
            targetY = bestY;
            targetEnergy = lowestEnergy;
        }
    }

    public override void OnHitBot(HitBotEvent evt)
    {
        Forward(50);

        if (knownEnemies.ContainsKey(evt.VictimId))
        {
            var enemy = knownEnemies[evt.VictimId];
            knownEnemies[evt.VictimId] = (enemy.x, enemy.y, evt.Energy);
        }
    }

    public override void OnBotDeath(BotDeathEvent evt)
    {

        knownEnemies.Remove(evt.VictimId);
        if (evt.VictimId == targetId)
        {
            targetId = -1;
            targetX = -1;
            targetY = -1;
            targetEnergy = double.MaxValue;
            SelectWeakestTarget(); 
        }
    }

    public override void OnHitByBullet(HitByBulletEvent evt)
    {

        if (Energy < 20)
        {
            double bearing = CalcBearing(evt.Bullet.Direction);
            TurnLeft(90 - bearing);
            Forward(60);
        }
    }

    public override void OnHitWall(HitWallEvent evt)
    {
        Back(20);
        TurnRight(30);
    }
}
