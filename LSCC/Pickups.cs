using GTANetworkAPI;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace RNC
{
    public class Pickup
    {
        public readonly int id;

        public readonly uint model;

        public readonly Vector3 position;

        public readonly float pickupRange;

        public readonly uint dimension;

        public readonly ColShape colshape;

        public readonly GTANetworkAPI.Object prop;

        public readonly long createTick;

        public readonly float[] lightData;

        public readonly bool rotation;

        public Action<Player> OnPickup { get; set; }

        public void Destroy()
        {
            Pickups.Items.Remove(this);

            this.colshape.Delete();
            if(this.prop != null) this.prop.Delete();
        }
     
        public Pickup(uint model, Vector3 position, float range, uint dimension, bool rotate = true, float[] lights = null)
        {
            Pickups.Items.Add(this);

            this.id = Pickups.Items.IndexOf(this);

            this.model = model;
            this.position = position;
            this.pickupRange = range;
            this.dimension = dimension;
            this.createTick = Environment.TickCount64;
            this.rotation = rotate;
            this.lightData = null;

            this.OnPickup += ((player) => this.Destroy());

            this.colshape = NAPI.ColShape.CreateSphereColShape(this.position, this.pickupRange, this.dimension);
            this.colshape.SetSharedData("BelongsToPickup", this.id);

            if (this.model != 0)
            {
                this.prop = NAPI.Object.CreateObject(this.model, this.position, new Vector3(0, 0, 0), 255, this.dimension);
                this.prop.SetSharedData("BelongsToPickup", this.id);

                if (rotate)
                {
                    this.prop.SetSharedData("PropRotation", this.rotation);
                }
                if (lights != null)
                {
                    // R, G, B, RANGE, INTENSITY, SHADOW
                    this.lightData = new float[6] { lights[0], lights[1], lights[2], lights[3], lights[4], lights[5] };
                    this.prop.SetSharedData("PropLighting", this.lightData);
                }
            }
            else this.prop = null;
        }
    }
    public static class Pickups
    {
        public static float[] ToPropLighting(float red, float green, float blue, float range, float intensity, float shadow)
        {
            return new float[6] { red, green, blue, range, intensity, shadow };
        }

        public static List<Pickup> Items = new List<Pickup>();
    }
    public class PickupHandler : Script
    {
        [ServerEvent(Event.PlayerEnterColshape)]
        public void OnPlayerEnterColshape(ColShape colshape, Player player)
        {
            Pickup pickup = Pickups.Items.Find(a => a.colshape == colshape);
            if(pickup != null && (Environment.TickCount64 - pickup.createTick) >= 1500)
            {
                pickup.OnPickup(player);
            }
        }
    }
}
