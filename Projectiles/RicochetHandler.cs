using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using ArmoredMounts.NPCs;

namespace ArmoredMounts.Projectiles
{
    /// <summary>
    /// Intercepta todos los proyectiles del juego.
    /// Si impactan un ArmoredGolem, los rebota sin dañar al jugador.
    /// El proyectil rebotado puede dañar a otros enemigos.
    /// </summary>
    public class RicochetHandler : GlobalProjectile
    {
        public override void OnHitNPC(Projectile projectile, NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (target.ModNPC is ArmoredGolem golem)
            {
                bool isFire = projectile.type == ProjectileID.Flames
                           || projectile.type == ProjectileID.FlamesTrap
                           || projectile.type == ProjectileID.MolotovFire
                           || projectile.type == ProjectileID.MolotovFire2
                           || projectile.type == ProjectileID.HellfireArrow;

                // Aplicar calor si es fuego
                if (isFire)
                    target.AddBuff(BuffID.OnFire, 120);

                bool isDynamite = projectile.type == ProjectileID.Dynamite
                               || projectile.type == ProjectileID.BouncyDynamite;

                // Dinamita siempre penetra, no rebota
                if (isDynamite)
                    return;

                // Todo lo demás rebota
                projectile.velocity.X = -projectile.velocity.X * 0.7f;
                projectile.velocity.Y = -projectile.velocity.Y * 0.7f;
                projectile.friendly   = false;
                projectile.hostile    = true;  // daña enemigos
                projectile.owner      = 255;   // no daña al jugador
            }
        }
    }
}