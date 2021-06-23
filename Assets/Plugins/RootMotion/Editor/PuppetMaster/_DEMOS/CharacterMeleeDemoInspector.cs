using UnityEngine;
using UnityEditor;
using System.Collections;
using RootMotion.Dynamics;
using UnityEditor.SceneManagement;

namespace RootMotion.Demos {
	
	[CustomEditor(typeof(CharacterMeleeDemo))]
	public class CharacterMeleeDemoInspector : Editor {
		
		private CharacterMeleeDemo script { get { return target as CharacterMeleeDemo; }}
		
		private GameObject replace;

		private static Color pro = new Color(0.7f, 0.9f, 0.5f, 1f);
		private static Color free = new Color(0.4f, 0.5f, 0.3f, 1f);
		
		public override void OnInspectorGUI() {
			GUI.changed = false;

			if (!Application.isPlaying) {
				GUI.color = EditorGUIUtility.isProSkin? pro: free;
				EditorGUILayout.BeginHorizontal();
				
				replace = (GameObject)EditorGUILayout.ObjectField("Replace Character Model", replace, typeof(GameObject), true);
				
				if (replace != null) {
					if (GUILayout.Button("Replace")) {
                        //Debug.Log("PropMuscle needs to be set up manually and assigned as 'Prop Muscle' in the CharacterMeleeDemo component on the character.");

                        //TODO Update to PropMuscle
                        /*
						PropRoot propRoot = script.propRoot;
						Vector3 localPosition = propRoot.transform.localPosition;
						Quaternion localRotation = propRoot.transform.localRotation;
						propRoot.transform.parent = null;
                        */

                        bool hasPropMuscle = script.propMuscle != null;
                        PropMuscle propMuscle = script.propMuscle;
                        Vector3 propMusclePosition = Vector3.zero;
                        Quaternion propMuscleRotation = Quaternion.identity;
                        Vector3 additionalPinOffset = Vector3.zero;

                        if (hasPropMuscle)
                        {
                            var cJ = propMuscle.GetComponent<ConfigurableJoint>().connectedBody.transform;
                            propMusclePosition = cJ.InverseTransformPoint(propMuscle.transform.position);
                            propMuscleRotation = Quaternion.Inverse(cJ.rotation) * propMuscle.transform.rotation;
                            additionalPinOffset = propMuscle.additionalPinOffset;
                        }

						CharacterPuppetInspector.ReplacePuppetModel(script as CharacterThirdPerson, replace);

                        if (hasPropMuscle)
                        {
                            Animator animator = script.characterAnimation.GetComponent<Animator>();
                            PuppetMaster puppetMaster = script.transform.parent.GetComponentInChildren<PuppetMaster>();
                            var rightHand = animator.GetBoneTransform(HumanBodyBones.RightHand);
                            var connectToJoint = GetJoint(puppetMaster, animator, HumanBodyBones.RightLowerArm);

                            PuppetMasterInspector.AddPropMuscle(puppetMaster, connectToJoint, connectToJoint.transform.TransformPoint(propMusclePosition), connectToJoint.transform.rotation * propMuscleRotation, additionalPinOffset, rightHand);
                            script.propMuscle = puppetMaster.muscles[puppetMaster.muscles.Length - 1].joint.GetComponent<PropMuscle>();

                            Debug.LogWarning("If bone orientations of the new and old models mismatch, PropMuscle position and rotation needs to be adjusted manually. This can be done by selecting the PropMuscle GameObject and moving/rotating it.");
                            Selection.activeGameObject = script.propMuscle.gameObject;
                        }

                        /*
						Animator animator = script.characterAnimation.GetComponent<Animator>();
						PuppetMaster puppetMaster = script.transform.parent.GetComponentInChildren<PuppetMaster>();

						propRoot.transform.parent = animator.GetBoneTransform(HumanBodyBones.RightHand);
						propRoot.transform.localPosition = localPosition;
						propRoot.transform.localRotation = localRotation;
						propRoot.puppetMaster = puppetMaster;
						propRoot.connectTo = GetRigidbody(puppetMaster, animator.GetBoneTransform(HumanBodyBones.RightLowerArm));
                        
						Debug.Log("You probably need to adjust the localPosition and localRotation of the Prop Root to match this character's hand.");
                        */
                        UserControlAI[] userControls = (UserControlAI[])GameObject.FindObjectsOfType<UserControlAI>();
						foreach (UserControlAI ai in userControls) {
							if (ai.moveTarget == null) {
								ai.moveTarget = script.transform.parent.GetComponentInChildren<PuppetMaster>().muscles[0].joint.transform;
                                EditorUtility.SetDirty(ai);
							}
						}

                        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
                        EditorUtility.SetDirty(script);
                    }
				}
				
				EditorGUILayout.EndHorizontal();
				GUI.color = Color.white;
			}
			
			DrawDefaultInspector();

			if (GUI.changed) EditorUtility.SetDirty(script);
		}

        private static ConfigurableJoint GetJoint(PuppetMaster puppetMaster, Animator animator, HumanBodyBones bone)
        {
            var boneTransform = animator.GetBoneTransform(bone);
            foreach (Muscle m in puppetMaster.muscles)
            {
                if (m.target == boneTransform) return m.joint;
            }
            return null;
        }

		private Rigidbody GetRigidbody(PuppetMaster puppetMaster, Transform target) {
			foreach (Muscle m in puppetMaster.muscles) {
				if (m.target == target) return m.joint.GetComponent<Rigidbody>();
			}
			return null;
		}
	}
}
