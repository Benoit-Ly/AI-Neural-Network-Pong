using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class ANNSaveData {

    public List<PerceptronSaveData> inputLayerWeights;
    public List<PerceptronSaveData> hiddenLayerWeights;
}
