﻿using System;
using UnityEditor.AnimatedValues;
using UnityEditor.Experimental.Rendering;
using UnityEditorInternal;
using UnityEngine.Events;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;

namespace UnityEditor.Experimental.Rendering
{
    partial class HDReflectionProbeEditor
    {
        internal class SerializedReflectionProbe
        {
            internal SerializedObject so;

            internal SerializedProperty mode;
            internal SerializedProperty renderDynamicObjects;
            internal SerializedProperty customBakedTexture; 
            internal SerializedProperty refreshMode;
            internal SerializedProperty timeSlicingMode;
            internal SerializedProperty intensityMultiplier;
            internal SerializedProperty blendDistance;
            internal SerializedProperty boxSize;
            internal SerializedProperty boxOffset;
            
            internal SerializedProperty influenceShape;
            internal SerializedProperty influenceSphereRadius; 
            internal SerializedProperty useSeparateProjectionVolume;
            internal SerializedProperty boxReprojectionVolumeSize;
            internal SerializedProperty boxReprojectionVolumeCenter;
            internal SerializedProperty sphereReprojectionVolumeRadius;

            public SerializedReflectionProbe(SerializedObject so, SerializedObject addso)
            {
                this.so = so;

                mode = so.FindProperty("m_Mode");
                customBakedTexture = so.FindProperty("m_CustomBakedTexture"); 
                renderDynamicObjects = so.FindProperty("m_RenderDynamicObjects");
                refreshMode = so.FindProperty("m_RefreshMode");
                timeSlicingMode = so.FindProperty("m_TimeSlicingMode");
                intensityMultiplier = so.FindProperty("m_IntensityMultiplier"); 
                blendDistance = so.FindProperty("m_BlendDistance");
                boxSize = so.FindProperty("m_BoxSize");
                boxOffset = so.FindProperty("m_BoxOffset");

                influenceShape = addso.Find((HDAdditionalReflectionData d) => d.m_InfluenceShape);
                influenceSphereRadius = addso.Find((HDAdditionalReflectionData d) => d.m_InfluenceSphereRadius);
                useSeparateProjectionVolume = addso.Find((HDAdditionalReflectionData d) => d.m_UseSeparateProjectionVolume);
                boxReprojectionVolumeSize = addso.Find((HDAdditionalReflectionData d) => d.m_BoxReprojectionVolumeSize);
                boxReprojectionVolumeCenter = addso.Find((HDAdditionalReflectionData d) => d.m_BoxReprojectionVolumeCenter);
                sphereReprojectionVolumeRadius = addso.Find((HDAdditionalReflectionData d) => d.m_SphereReprojectionVolumeRadius);
            }
        }

        [Flags]
        internal enum Operation
        {
            None = 0,
            UpdateOldLocalSpace = 1 << 0,
            FitVolumeToSurroundings = 1 << 1
        }

        internal class UIState
        {
            AnimBool[] m_ModeSettingsDisplays = new AnimBool[Enum.GetValues(typeof(ReflectionProbeMode)).Length];
            AnimBool[] m_InfluenceShapeDisplays = new AnimBool[Enum.GetValues(typeof(ReflectionInfluenceShape)).Length];

            Editor owner { get; set; }
            Operation operations { get; set; }
            public AnimBool useSeparateProjectionVolumeDisplay { get; private set; }
            public bool HasOperation(Operation op) { return (operations & op) == op; }
            public void ClearOperation(Operation op) { operations &= ~op; }
            public void AddOperation(Operation op) { operations |= op; }

            public bool HasAndClearOperation(Operation op)
            {
                var has = HasOperation(op);
                ClearOperation(op);
                return has;
            }

            public bool sceneViewEditing
            {
                get { return IsReflectionProbeEditMode(EditMode.editMode) && EditMode.IsOwner(owner); }
            }

            internal UIState()
            {
                for (var i = 0; i < m_ModeSettingsDisplays.Length; i++)
                    m_ModeSettingsDisplays[i] = new AnimBool();
                for (var i = 0; i < m_InfluenceShapeDisplays.Length; i++)
                    m_InfluenceShapeDisplays[i] = new AnimBool();
                useSeparateProjectionVolumeDisplay = new AnimBool();
            }

            internal void Reset(
                Editor owner, 
                UnityAction repaint, 
                SerializedReflectionProbe p)
            {
                this.owner = owner;
                operations = 0;

                for (var i = 0; i < m_ModeSettingsDisplays.Length; i++)
                {
                    m_ModeSettingsDisplays[i].valueChanged.RemoveAllListeners();
                    m_ModeSettingsDisplays[i].valueChanged.AddListener(repaint);
                    m_ModeSettingsDisplays[i].value = p.mode.intValue == i;
                }

                for (var i = 0; i < m_InfluenceShapeDisplays.Length; i++)
                {
                    m_InfluenceShapeDisplays[i].valueChanged.RemoveAllListeners();
                    m_InfluenceShapeDisplays[i].valueChanged.AddListener(repaint);
                    m_InfluenceShapeDisplays[i].value = p.influenceShape.intValue == i;
                }

                useSeparateProjectionVolumeDisplay.valueChanged.RemoveAllListeners();
                useSeparateProjectionVolumeDisplay.valueChanged.AddListener(repaint);
                useSeparateProjectionVolumeDisplay.value = p.useSeparateProjectionVolume.boolValue;
            }

            public float GetModeFaded(ReflectionProbeMode mode)
            {
                return m_ModeSettingsDisplays[(int)mode].faded;
            }

            public void SetModeTarget(int value)
            {
                for (var i = 0; i < m_ModeSettingsDisplays.Length; i++)
                    m_ModeSettingsDisplays[i].target = i == value;
            }

            public float GetShapeFaded(ReflectionInfluenceShape value)
            {
                return m_InfluenceShapeDisplays[(int)value].faded;
            }

            public void SetShapeTarget(int value)
            {
                for (var i = 0; i < m_InfluenceShapeDisplays.Length; i++)
                    m_InfluenceShapeDisplays[i].target = i == value;
            }

            static bool IsReflectionProbeEditMode(EditMode.SceneViewEditMode editMode)
            {
                return editMode == EditMode.SceneViewEditMode.ReflectionProbeBox || editMode == EditMode.SceneViewEditMode.Collider || editMode == EditMode.SceneViewEditMode.GridBox ||
                    editMode == EditMode.SceneViewEditMode.ReflectionProbeOrigin;
            }
        }
    }
}