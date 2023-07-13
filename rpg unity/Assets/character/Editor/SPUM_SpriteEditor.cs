using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;

[CustomEditor(typeof(SPUM_SpriteList))]
//[CanEditMultipleObjects]
public class SPUM_SpriteEditor : Editor
{
    // Start is called before the first frame update
    public override void OnInspectorGUI()
    {

        SPUM_SpriteList SPB = (SPUM_SpriteList)target;

        bool dirChk = Directory.Exists("Assets/character/SPUM_Sprites/Items");
        
        if(!dirChk)
        {
            EditorGUILayout.HelpBox("Your SPUM sprites data were deleted from Resources folder, Please check data is available",MessageType.Error);
        }
        else
        {
            if(SPB._backList[0] == null)
            {
                EditorGUILayout.HelpBox("Your sprite sync data deleted, please resync",MessageType.Error);
                if (GUILayout.Button("Sync Sprites",GUILayout.Height(50))) 
                {
                    SPB.ResyncData();
                }
            }
            else
            {
                
                if (GUILayout.Button("Resync Sprites",GUILayout.Height(50))) 
                {
                    SPB.ResyncData();
                }
                base.OnInspectorGUI();
            }
            
        }
       
    }
}
