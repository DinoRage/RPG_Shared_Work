using AwesomeTechnologies.BillboardSystem;
using Unity.Collections;
using UnityEngine;

namespace AwesomeTechnologies.VegetationSystem
{
    public partial class VegetationSystemPro
    {
        private void CompactCache()
        {
            CompactVegetationCellCache();
            //CompactBillboardCellCache();
        }

        private void CompactVegetationCellCache()
        {                       
            for (int i = 0; i <= LoadedVegetationCellList.Count - 1; i++)
            {
                VegetationCell vegetationCell = LoadedVegetationCellList[i];
                vegetationCell.FlagForRemoval = vegetationCell.Prepared;
            }

            for (int i = 0; i <= VegetationStudioCameraList.Count - 1; i++)
            {
                VegetationStudioCamera vegetationStudioCamera = VegetationStudioCameraList[i];
                if (!vegetationStudioCamera.Enabled) continue;

                for (int j = 0; j <= vegetationStudioCamera.PotentialVisibleVegetationCellList.Count - 1; j++)
                {
                    VegetationCell vegetationCell = vegetationStudioCamera.PotentialVisibleVegetationCellList[j];
                    vegetationCell.FlagForRemoval = false;
                }
            }

            PredictiveCellLoader.RemoveCellsFlaggedForRemoval();


            int clearCellCounter = 0;
            for (int i = LoadedVegetationCellList.Count - 1; i >= 0; i--)
            {
                if (LoadedVegetationCellList[i].FlagForRemoval)
                {
                    clearCellCounter++;
                    LoadedVegetationCellList[i].ClearCache();
                    OnClearCacheVegetationCellDelegate?.Invoke(this, LoadedVegetationCellList[i]);
                    LoadedVegetationCellList.RemoveAtSwapBack(i);
                }
            }

            if (clearCellCounter > 0)
            {
               // Debug.Log("Cleared " + clearCellCounter  + " VegetationCells");
            }
        }

        void CompactBillboardCellCache()
        {
            // for (int i = 0; i <= BillboardCellList.Count - 1; i++)
            // {
            //     BillboardCell billboardCell = BillboardCellList[i];
            //     billboardCell.FlagForRemoval = billboardCell.Prepared;
            // }
            //
            // for (int i = 0; i <= VegetationStudioCameraList.Count - 1; i++)
            // {
            //     VegetationStudioCamera vegetationStudioCamera = VegetationStudioCameraList[i];
            //     if (!vegetationStudioCamera.Enabled) continue;
            //
            //     for (int j = 0; j <= vegetationStudioCamera.PotentialVisibleBillboardCellList.Count - 1; j++)
            //     {
            //         BillboardCell billboardCell = vegetationStudioCamera.PotentialVisibleBillboardCellList[j];
            //         billboardCell.FlagForRemoval = false;
            //     }
            // }
            //
            // for (int i = BillboardCellList.Count - 1; i >= 0; i--)
            // {
            //     if (BillboardCellList[i].FlagForRemoval)
            //     {
            //         BillboardCellList[i].Dispose();
            //     }
            // }
        }
    }
}
