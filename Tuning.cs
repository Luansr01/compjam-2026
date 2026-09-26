using Godot;

public partial class Tuning : Node
{
	public static Tuning Instance { get; private set; }

	[ExportGroup("Spawning")]
	[Export] f64 outerRadius = 500.0;
	[Export] f64 innerRadius = 300.0;
	[Export] f64 spawnRate = 5.0;
	[Export] i32 spawnCap = 5;

	[ExportGroup("Difficulty Ramp")]
	[Export] f64 enemiesPerMinute = 10.0;
	[Export] f64 rateGrowthPerMinute = 1.1;
	[Export] f64 speedPerMinute = 0.20;
	[Export] f64 damagePerMinute = 0.35;
	[Export] f64 healthPerMinute = 0.0;

	[ExportGroup("Threat (Nexus Hit)")]
	[Export] f64 threatPerHit = 0.15;
	[Export] f64 threatDecayPerSecond = 0.25;
	[Export] f64 threatCapBonus = 12.0;
	[Export] f64 threatRateBonus = 1.2;

	[ExportGroup("Score")]
	[Export] i32 pointsPerKill = 1;
	[Export] f64 pointsPerMinute = 3.0;
	[Export] f64 lowHealthScoreBonus = 1.5;

	[ExportGroup("Upgrades")]
	[Export] f64 upgradeCostGrowth = 1.8;
	[Export] i32 upgradeBaseDamage = 5;
	[Export] i32 upgradeBaseAttackSpeed = 10;
	[Export] i32 upgradeBaseMoveSpeed = 15;
	[Export] i32 upgradeBaseHeal = 20;
	[Export] i32 upgradeBaseNexusHeal = 30;
	[Export] f64 damagePerLevel = 0.25;
	[Export] f64 cooldownPerLevel = 0.10;
	[Export] f64 moveSpeedPerLevel = 0.15;
	[Export] f64 healFractionPerLevel = 0.10;

	[ExportGroup("Active Items")]
	[Export] f64 itemCostGrowth = 1.4;
	[Export] f64 tauntBaseCost = 40.0;
	[Export] f64 tauntDuration = 8.0;
	[Export] f64 tauntDamageScale = 2.0;

	[Export] f64 shockwaveBaseCost = 60.0;
	[Export] f64 shockwaveCooldown = 6.0;
	[Export] f64 shockwaveRadius = 1800.0;
	[Export] f64 shockwaveDamage = 250.0;
	[Export] f64 shockwaveKnockback = 1800.0;
	[Export] f64 shockwaveSlowFactor = 0.35;
	[Export] f64 shockwaveSlowDuration = 4.0;

	[ExportGroup("Nexus")]
	[Export] f64 drainEnabled = 1.0;
	[Export] f64 drainStartPerSecond = 3.0;
	[Export] f64 drainPerMinute = 6.0;
	[Export] f64 pityThreshold = 0.35;
	[Export] f64 pityFloor = 0.25;
	[Export] f64 fullHealthDamageScale = 1.6;

	[ExportGroup("Player Desperation")]
	[Export] f64 desperationThreshold = 0.4;
	[Export] f64 desperationBonus = 0.6;
	[Export] f64 desperationDamageFloor = 0.3;
	[ExportGroup("Explosions")]
	[Export] f64 explosionRadius = 220.0;
	[Export] f64 explosionDamage = 60.0;
	[Export] f64 explosionKnockback = 700.0;
	[Export] f64 explosionSlowFactor = 0.45;
	[Export] f64 explosionSlowDuration = 2.0;
	[Export] f64 explosionSeconds = 0.45;
	[Export] f64 explosionMinShare = 0.0;

	[ExportGroup("Feedback")]
	[Export] f64 hitFlashSeconds = 0.15;

	[Export] f64 vignetteLowHealth = 0.45;
	[Export] f64 vignettePulseHz = 2.2;
	[Export] f64 vignetteMaxAlpha = 0.55;

	[Export] f64 pulseSeconds = 0.85;
	[Export] i32 pulseRingCount = 8;
	[Export] f64 pulseCoreWidth = 110.0;
	[Export] f64 pulseBandWidth = 80.0;
	[Export] Color pulseCoreColor = new(1.00f, 1.00f, 1.00f, 1.00f);
	[Export] Color pulseEdgeColor = new(0.45f, 0.82f, 1.00f, 0.95f);

	public override void _EnterTree() => Instance = this;

	public override void _ExitTree()
	{
		if (Instance == this) Instance = null;
	}

	// Every accessor falls back to the field's own default when Instance is null,
	// which is the case in the editor: autoloads are not instantiated there, and
	// the spawner gizmo is a [Tool] script that reads the radii.

	public static f64 OuterRadius => Instance?.outerRadius ?? 500.0;
	public static f64 InnerRadius => Instance?.innerRadius ?? 300.0;
	public static f64 SpawnRate => Instance?.spawnRate ?? 5.0;
	public static i32 SpawnCap => Instance?.spawnCap ?? 5;

	public static f64 EnemiesPerMinute => Instance?.enemiesPerMinute ?? 10.0;
	public static f64 RateGrowthPerMinute => Instance?.rateGrowthPerMinute ?? 1.1;
	public static f64 SpeedPerMinute => Instance?.speedPerMinute ?? 0.20;
	public static f64 DamagePerMinute => Instance?.damagePerMinute ?? 0.35;
	public static f64 HealthPerMinute => Instance?.healthPerMinute ?? 0.0;

	public static f64 ThreatPerHit => Instance?.threatPerHit ?? 0.15;
	public static f64 ThreatDecayPerSecond => Instance?.threatDecayPerSecond ?? 0.25;
	public static f64 ThreatCapBonus => Instance?.threatCapBonus ?? 12.0;
	public static f64 ThreatRateBonus => Instance?.threatRateBonus ?? 1.2;

	public static i32 PointsPerKill => Instance?.pointsPerKill ?? 1;
	public static f64 PointsPerMinute => Instance?.pointsPerMinute ?? 3.0;
	public static f64 LowHealthScoreBonus => Instance?.lowHealthScoreBonus ?? 1.5;

	public static f64 UpgradeCostGrowth => Instance?.upgradeCostGrowth ?? 1.8;
	public static i32 UpgradeBaseDamage => Instance?.upgradeBaseDamage ?? 5;
	public static i32 UpgradeBaseAttackSpeed => Instance?.upgradeBaseAttackSpeed ?? 10;
	public static i32 UpgradeBaseMoveSpeed => Instance?.upgradeBaseMoveSpeed ?? 15;
	public static i32 UpgradeBaseHeal => Instance?.upgradeBaseHeal ?? 20;
	public static i32 UpgradeBaseNexusHeal => Instance?.upgradeBaseNexusHeal ?? 30;
	public static f64 DamagePerLevel => Instance?.damagePerLevel ?? 0.25;
	public static f64 CooldownPerLevel => Instance?.cooldownPerLevel ?? 0.10;
	public static f64 MoveSpeedPerLevel => Instance?.moveSpeedPerLevel ?? 0.15;
	public static f64 HealFractionPerLevel => Instance?.healFractionPerLevel ?? 0.10;

	public static f64 ItemCostGrowth => Instance?.itemCostGrowth ?? 1.4;
	public static f64 TauntBaseCost => Instance?.tauntBaseCost ?? 40.0;
	public static f64 TauntDuration => Instance?.tauntDuration ?? 8.0;
	public static f64 TauntDamageScale => Instance?.tauntDamageScale ?? 2.0;

	public static f64 ShockwaveBaseCost => Instance?.shockwaveBaseCost ?? 60.0;
	public static f64 ShockwaveCooldown => Instance?.shockwaveCooldown ?? 6.0;
	public static f64 ShockwaveRadius => Instance?.shockwaveRadius ?? 1800.0;
	public static f64 ShockwaveDamage => Instance?.shockwaveDamage ?? 250.0;
	public static f64 ShockwaveKnockback => Instance?.shockwaveKnockback ?? 1800.0;
	public static f64 ShockwaveSlowFactor => Instance?.shockwaveSlowFactor ?? 0.35;
	public static f64 ShockwaveSlowDuration => Instance?.shockwaveSlowDuration ?? 4.0;

	public static bool DrainEnabled => Instance == null || Instance.drainEnabled != 0.0;
	public static f64 DrainStartPerSecond => Instance?.drainStartPerSecond ?? 3.0;
	public static f64 DrainPerMinute => Instance?.drainPerMinute ?? 6.0;
	public static f64 PityThreshold => Instance?.pityThreshold ?? 0.35;
	public static f64 PityFloor => Instance?.pityFloor ?? 0.25;
	public static f64 FullHealthDamageScale => Instance?.fullHealthDamageScale ?? 1.6;

	public static f64 DesperationThreshold => Instance?.desperationThreshold ?? 0.4;
	public static f64 DesperationBonus => Instance?.desperationBonus ?? 0.6;
	public static f64 DesperationDamageFloor => Instance?.desperationDamageFloor ?? 0.3;
	public static f64 HitFlashSeconds => Instance?.hitFlashSeconds ?? 0.3;

	public static f64 ExplosionRadius => Instance?.explosionRadius ?? 220.0;
	public static f64 ExplosionDamage => Instance?.explosionDamage ?? 60.0;
	public static f64 ExplosionKnockback => Instance?.explosionKnockback ?? 700.0;
	public static f64 ExplosionSlowFactor => Instance?.explosionSlowFactor ?? 0.45;
	public static f64 ExplosionSlowDuration => Instance?.explosionSlowDuration ?? 2.0;
	public static f64 ExplosionSeconds => Instance?.explosionSeconds ?? 0.45;
	public static f64 ExplosionMinShare => Instance?.explosionMinShare ?? 0.0;

	public static f64 VignetteLowHealth => Instance?.vignetteLowHealth ?? 0.45;
	public static f64 VignettePulseHz => Instance?.vignettePulseHz ?? 2.2;
	public static f64 VignetteMaxAlpha => Instance?.vignetteMaxAlpha ?? 0.55;

	public static f64 PulseSeconds => Instance?.pulseSeconds ?? 0.85;
	public static i32 PulseRingCount => Instance?.pulseRingCount ?? 8;
	public static f64 PulseCoreWidth => Instance?.pulseCoreWidth ?? 110.0;
	public static f64 PulseBandWidth => Instance?.pulseBandWidth ?? 80.0;
	public static Color PulseCoreColor => Instance?.pulseCoreColor ?? new Color(1.00f, 1.00f, 1.00f, 1.00f);
	public static Color PulseEdgeColor => Instance?.pulseEdgeColor ?? new Color(0.45f, 0.82f, 1.00f, 0.95f);
}
