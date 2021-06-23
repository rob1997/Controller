using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace RootMotion.Dynamics
{

    [CustomEditor(typeof(PuppetMasterLite))]
    public class PuppetMasterLiteInspector : Editor
    {
        private PuppetMasterLite script { get { return target as PuppetMasterLite; } }
        private MonoScript monoScript;

        void OnEnable()
        {
            if (!Application.isPlaying)
            {
                monoScript = MonoScript.FromMonoBehaviour(script);
                int currentExecutionOrder = MonoImporter.GetExecutionOrder(monoScript);
                if (currentExecutionOrder != 10101) MonoImporter.SetExecutionOrder(monoScript, 10101);
            }
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            if (script.muscles.Length == 0)
            {
                var p = script.GetComponent<PuppetMaster>();
                if (p == null) return;

                EditorGUILayout.Space();
                if (GUILayout.Button("Convert PuppetMaster to PuppetMasterLite"))
                {
                    script.targetRoot = p.targetRoot;
                    script.fixTargetTransforms = p.fixTargetTransforms;
                    script.blendTime = p.blendTime;
                    script.mappingWeight = p.mappingWeight;
                    script.pinWeight = p.pinWeight;
                    script.muscleWeight = p.muscleWeight;
                    script.muscleSpring = p.muscleSpring;
                    script.muscleDamper = p.muscleDamper;
                    script.updateJointAnchors = p.updateJointAnchors;
                    script.angularPinning = p.angularPinning;

                    script.muscles = new MuscleLite[p.muscles.Length];
                    for (int i = 0; i < script.muscles.Length; i++)
                    {
                        script.muscles[i] = new MuscleLite();
                        script.muscles[i].joint = p.muscles[i].joint;
                        script.muscles[i].target = p.muscles[i].target;
                        script.muscles[i].pinWeightMlp = p.muscles[i].props.pinWeight;
                        script.muscles[i].muscleWeightMlp = p.muscles[i].props.muscleWeight;
                        script.muscles[i].muscleDamperMlp = p.muscles[i].props.muscleDamper;
                        script.muscles[i].mappingWeightMlp = p.muscles[i].props.mappingWeight;
                        //script.muscles[i].mapPosition = p.muscles[i].props.mapPosition;
                    }

                    var behaviours = p.transform.parent != null ? p.transform.parent.GetComponentsInChildren<BehaviourBase>() : new BehaviourBase[0];
                    for (int i = 0; i < behaviours.Length; i++)
                    {
                        behaviours[i].gameObject.SetActive(false);
                    }

                    DestroyImmediate(p);

                    serializedObject.ApplyModifiedProperties();
                }
            }
            else
            {
                EditorGUILayout.Space();
                if (GUILayout.Button("Convert PuppetMasterLite to PuppetMaster"))
                {
                    var p = script.gameObject.AddComponent<PuppetMaster>();

                    p.targetRoot = script.targetRoot;
                    p.fixTargetTransforms = script.fixTargetTransforms;
                    p.blendTime = script.blendTime;
                    p.mappingWeight = script.mappingWeight;
                    p.pinWeight = script.pinWeight;
                    p.muscleWeight = script.muscleWeight;
                    p.muscleSpring = script.muscleSpring;
                    p.muscleDamper = script.muscleDamper;
                    p.updateJointAnchors = script.updateJointAnchors;
                    p.angularPinning = script.angularPinning;

                    p.muscles = new Muscle[script.muscles.Length];
                    for (int i = 0; i < p.muscles.Length; i++)
                    {
                        p.muscles[i] = new Muscle();
                        p.muscles[i].joint = script.muscles[i].joint;
                        p.muscles[i].target = script.muscles[i].target;
                        p.muscles[i].props.pinWeight = script.muscles[i].pinWeightMlp;
                        p.muscles[i].props.muscleWeight = script.muscles[i].muscleWeightMlp;
                        p.muscles[i].props.muscleDamper = script.muscles[i].muscleDamperMlp;
                        p.muscles[i].props.mappingWeight = script.muscles[i].mappingWeightMlp;
                        //p.muscles[i].props.mapPosition = script.muscles[i].mapPosition;
                    }

                    DestroyImmediate(script);
                    return;
                }
            }
        }
    }
}
