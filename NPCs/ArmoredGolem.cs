using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria.GameContent;

namespace ArmoredMounts.NPCs
{
    /// <summary>
    /// Enemigo blindado base del mod ArmoredMounts.
    /// Inmune a casi todo, pero el calor debilita su armadura metálica.
    /// Cuando está sobrecalentado, ciertos ataques lo penetran.
    /// </summary>
    public class ArmoredGolem : ModNPC
    {
        // --- CONSTANTES DE RESISTENCIA ---
        private const float MELEE_DAMAGE_REDUCTION = 0f;   // melee no hace nada
        private const float HEAT_MELEE_REDUCTION   = 0.3f; // sobrecalentado: melee hace 30%

        // --- CONSTANTES DE CALOR ---
        private const int HEAT_TICKS_REQUIRED = 180; // 3 segundos para sobrecalentar
        private const int COOL_TICKS_REQUIRED = 300; // 5 segundos para enfriarse

        // --- ESTADO INTERNO ---
        private int heatAccumulated = 0;
        private int cooldownTimer   = 0;
        public bool IsOverheated    = false;

        public override void SetStaticDefaults()
        {
            Main.npcFrameCount[Type] = 1;
        }

        public override void SetDefaults()
        {
            NPC.width           = 40;
            NPC.height          = 40;
            NPC.damage          = 25;
            NPC.defense         = 30;
            NPC.lifeMax         = 500;
            NPC.HitSound        = SoundID.NPCHit4;
            NPC.DeathSound      = SoundID.NPCDeath14;
            NPC.value           = 100f;
            NPC.knockBackResist = 0f;
            NPC.aiStyle         = 3;
        }

        /// <summary>
        /// Se ejecuta cada tick (60 veces por segundo).
        /// Aquí manejamos el sistema de calor.
        /// </summary>
        public override void AI()
        {
            base.AI();
            HandleHeatSystem();

            // DEBUG TEMPORAL
            if (Main.GameUpdateCount % 60 == 0)
            {
                Main.NewText(IsOverheated ? "GOLEM: SOBRECALENTADO" : "GOLEM: Normal", 
                            IsOverheated ? Color.OrangeRed : Color.Gray);
            }
        }
        /// <summary>
        /// Cambia el sprite según el estado de calor en tiempo real.
        /// </summary>
        public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            string texturePath = IsOverheated
                ? "ArmoredMounts/NPCs/ArmoredGolem_Overheated"
                : "ArmoredMounts/NPCs/ArmoredGolem";

            var texture = ModContent.Request<Texture2D>(texturePath).Value;
            spriteBatch.Draw(texture, NPC.Center - screenPos, null, drawColor, NPC.rotation, texture.Bounds.Size() / 2f, NPC.scale, SpriteEffects.None, 0f);

            return false; // false = no dibuja el sprite por defecto
        }

        /// <summary>
        /// Detecta si el Golem está en contacto con lava o fuego
        /// y actualiza su estado de calor en consecuencia.
        /// </summary>
        private void HandleHeatSystem()
        {
            bool isInHeat = NPC.onFire || NPC.onFire2 || NPC.lavaWet;

            if (isInHeat)
            {
                heatAccumulated++;
                cooldownTimer = 0;

                if (heatAccumulated >= HEAT_TICKS_REQUIRED)
                    IsOverheated = true;
            }
            else
            {
                if (IsOverheated)
                {
                    cooldownTimer++;
                    if (cooldownTimer >= COOL_TICKS_REQUIRED)
                    {
                        IsOverheated    = false;
                        heatAccumulated = 0;
                        cooldownTimer   = 0;
                    }
                }
                else
                {
                    heatAccumulated = 0;
                }
            }
        }

        // --- MELEE ---
        public override void ModifyHitByItem(Player player, Item item, ref NPC.HitModifiers modifiers)
        {
            if (IsOverheated)
                modifiers.FinalDamage *= HEAT_MELEE_REDUCTION;
            else
                modifiers.FinalDamage *= MELEE_DAMAGE_REDUCTION;
        }

        // --- PROYECTILES ---
        public override void ModifyHitByProjectile(Projectile projectile, ref NPC.HitModifiers modifiers)
        {
            bool isHighVelocity = projectile.type == ProjectileID.BulletHighVelocity;
            bool isGrenade      = projectile.type == ProjectileID.Grenade;
            bool isBomb         = projectile.type == ProjectileID.Bomb;
            bool isDynamite     = projectile.type == ProjectileID.Dynamite;
            bool isMagic        = projectile.DamageType == DamageClass.Magic;

            if (IsOverheated)
            {
                if (isHighVelocity)
                    modifiers.FinalDamage *= 1f;   // daño completo
                else if (isDynamite)
                    modifiers.FinalDamage *= 1f;   // daño completo
                else if (isBomb)
                    modifiers.FinalDamage *= 0.4f; // 40%
                else if (isGrenade)
                    modifiers.FinalDamage *= 0.1f; // 10%
                else if (isMagic)
                    modifiers.FinalDamage *= 0.6f; // 60%
                else
                    modifiers.FinalDamage *= 0f;   // todo lo demás rebota
            }
            else
            {
                modifiers.FinalDamage *= 0f; // inmune a todo
            }
        }
    }
}