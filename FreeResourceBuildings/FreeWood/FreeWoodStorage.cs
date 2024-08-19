using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TUNING;
using UnityEngine;

namespace FreeResourceBuildings
{
    public class FreeWoodStorage : KMonoBehaviour, ISim4000ms
    {


        [MyCmpReq]
        public Storage storage;

        public Element woodElement;

        protected override void OnPrefabInit()
        {
            base.OnPrefabInit();

        }


        protected override void OnSpawn()
        {
            base.OnSpawn();
            woodElement = ElementLoader.FindElementByHash(SimHashes.WoodLog);
            // this.GetComponent<KAnimControllerBase>().Play((HashedString)"off");
        }

        public void Sim4000ms(float dt)
        {
            var amount = storage.RemainingCapacity();
            if (amount > 10)
            {
                Vector3 position = this.transform.position + Vector3.up / 2f;

                int cell = Grid.PosToCell((KMonoBehaviour)this);
                var temperature = woodElement.lowTemp;
                if ((double)Grid.Mass[cell] > 0.0)
                    temperature = Grid.Temperature[cell];


                var go = woodElement.substance.SpawnResource(position, amount, temperature, byte.MaxValue, 0);


                storage.Store(go);
            }
        }

    }
}
