using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.Design.Serialization;
using UnityEditor;
using UnityEngine;

public class HPEquipSABRE_RE : HPEquippable, IMassObject
{
    public float Mass;

    [Serializable]
    public struct FullSpecList
    {
        public string listname;
        public float autoABThreshold;
        public float startupTime;
        public float maxThrust;
        public float fuelDrain;
        public float spoolRate;
        public bool lerpSpool;
        public float idleThrottle;
        [Header("Afterburner")]
        public float abThrustMult;
        public float abSpoolMult;
        public float abDrainMult;
        public bool progressiveAB;
        public float afterburnerRate;
        [Header("Thrust Effects")]
        public bool useSpeedCurve;
        public SOCurve speedCurve;
        public bool useAtmosCurve;
        [Tooltip("Thrust multiplier per atmospheric pressure (atm)")]
        public SOCurve atmosCurve;
        public bool useAoACurve;
        public SOCurve aoaCurve;
        [Header("Electrical")]
        public float startupDrain;
        public float alternatorChargeRate;
        [Header("Heat")]
        public float thrustHeatMult;
        public float abHeatAdd;
    }
    [Header("Engine Specs")]

    public FullSpecList NewSpecs;

    public FullSpecList OriginalSpecs;
    [Header("Controller Stuff")]

    public VehicleMaster vMaster;

    public FlightInfo vFInfo;

    public VTOLAutoPilot vAP;

    public List<ModuleEngine> engines;
    public List<GameObject> HiddenObjects;
    public List<GameObject> NewEngineObjects;
    public EngineEffects EngineFXL;
    public EngineEffects EngineFXR;
    public List<AtmosphericAudioSource> AAS;

    protected override void OnEquip()
    {
        Initialize();
        EquipEngines();
        SetOriginaMeshsEnabled(false);
    }

    public override void OnUnequip()
    {
        UnequipEngines();
        SetOriginaMeshsEnabled(true);
    }

    private void EquipEngines()
    {
        foreach (ModuleEngine engine in engines)
        {
            LoadSpecs(NewSpecs, engine);
        }
    }

    private void UnequipEngines()
    {
        foreach (ModuleEngine engine in engines)
        {
            LoadSpecs(OriginalSpecs, engine);
        }
    }

    public override void OnConfigAttach(LoadoutConfigurator configurator)
    {
        base.OnConfigAttach(configurator);
        Initialize(configurator);
        EquipEngines();
        SetOriginaMeshsEnabled(false);
    }

    public override void OnConfigDetach(LoadoutConfigurator configurator)
    {
        SetOriginaMeshsEnabled(true);
        UnequipEngines();
    }

    public float GetMass()
    {
        return Mass;
    }
    public void SaveOriginalSpecs(ModuleEngine engine)
    {
        OriginalSpecs = new FullSpecList();
        OriginalSpecs.listname = "Original";
        OriginalSpecs.autoABThreshold = engine.autoABThreshold;
        OriginalSpecs.startupTime = engine.startupTime;
        OriginalSpecs.maxThrust = engine.maxThrust;
        OriginalSpecs.fuelDrain = engine.fuelDrain;
        OriginalSpecs.spoolRate = engine.spoolRate;
        OriginalSpecs.lerpSpool = engine.lerpSpool;
        OriginalSpecs.idleThrottle = engine.idleThrottle;
        OriginalSpecs.abThrustMult = engine.abThrustMult;
        OriginalSpecs.abSpoolMult = engine.abSpoolMult;
        OriginalSpecs.abDrainMult = engine.abDrainMult;
        OriginalSpecs.progressiveAB = engine.progressiveAB;
        OriginalSpecs.afterburnerRate = engine.afterburnerRate;
        OriginalSpecs.useSpeedCurve = engine.useSpeedCurve;
        OriginalSpecs.speedCurve = engine.speedCurve;
        OriginalSpecs.useAtmosCurve = engine.useAtmosCurve;
        OriginalSpecs.atmosCurve = engine.atmosCurve;
        OriginalSpecs.useAoACurve = engine.useAoACurve;
        OriginalSpecs.aoaCurve = engine.aoaCurve;
        OriginalSpecs.startupDrain = engine.startupDrain;
        OriginalSpecs.alternatorChargeRate = engine.alternatorChargeRate;
        OriginalSpecs.thrustHeatMult = engine.thrustHeatMult;
        OriginalSpecs.abHeatAdd = engine.abHeatAdd;

    }

    public void LoadSpecs(FullSpecList speclist, ModuleEngine engine)
    {
        engine.autoABThreshold = speclist.autoABThreshold;
        engine.startupTime = speclist.startupTime;
        engine.maxThrust = speclist.maxThrust;
        engine.fuelDrain = speclist.fuelDrain;
        engine.spoolRate = speclist.spoolRate;
        engine.lerpSpool = speclist.lerpSpool;
        engine.idleThrottle = speclist.idleThrottle;
        engine.abThrustMult = speclist.abThrustMult;
        engine.abSpoolMult = speclist.abSpoolMult;
        engine.abDrainMult = speclist.abDrainMult;
        engine.progressiveAB = speclist.progressiveAB;
        engine.afterburnerRate = speclist.afterburnerRate;
        engine.useSpeedCurve = speclist.useSpeedCurve;
        engine.speedCurve = speclist.speedCurve;
        engine.useAtmosCurve = speclist.useAtmosCurve;
        engine.atmosCurve = speclist.atmosCurve;
        engine.useAoACurve = speclist.useAoACurve;
        engine.aoaCurve = speclist.aoaCurve;
        engine.startupDrain = speclist.startupDrain;
        engine.alternatorChargeRate = speclist.alternatorChargeRate;
        engine.thrustHeatMult = speclist.thrustHeatMult;
        engine.abHeatAdd = speclist.abHeatAdd;
    }

    public void SetOriginaMeshsEnabled(bool enabled)
    {
        foreach (GameObject go in HiddenObjects)
        {
            go.SetActive(enabled);
        }
        foreach (GameObject go in NewEngineObjects)
        {
            go.SetActive(!enabled);
        }
        if (!enabled)
        {
            foreach (ModuleEngine engine in engines)
            {
                if (engine.name == "fa26-leftEngine")
                {
                    engine.engineEffects = EngineFXL;
                }
                if (engine.name == "fa26-rightEngine")
                {
                    engine.engineEffects = EngineFXR;
                }
            }
        }
    }

    public void Initialize(LoadoutConfigurator configurator = null)
    {
        var wm = configurator ? configurator.wm : weaponManager;
        vMaster = wm.vm;
        foreach (ModuleEngine engine in vMaster.engines)
        {
            engines.Add(engine);
        }
        SaveOriginalSpecs(engines[0]);
        vFInfo = vMaster.GetComponentInParent<FlightInfo>();
        vAP = vMaster.GetComponentInParent<VTOLAutoPilot>();
        foreach (ModuleEngine engine in engines)
        {
            //Debug.Log("Found ModuleEngine : " + engine);
            foreach (Component component in engine.GetComponentsInChildren<Component>())
            {
                if (!HiddenObjects.Contains(component.gameObject))
                {
                    switch (component.gameObject.name)
                    {
                        case "vgEngineLowPoly":
                            HiddenObjects.Add(component.gameObject);
                            //Debug.Log("add vgEngineLowPoly to the HiddenObjects List");
                            break;
                        case "EngineFX":
                            HiddenObjects.Add(component.gameObject);
                            //Debug.Log("add EngineFX to the HiddenObjects List");
                            break;
                        case "toggleABAudio":
                            HiddenObjects.Add(component.gameObject);
                            //Debug.Log("add toggleABAudio to the HiddenObejcts List");
                            break;
                        case "toggleABAudio (1)":
                            HiddenObjects.Add(component.gameObject);
                            //Debug.Log("add toggleABAudio (1) to the HiddenObejects list");
                            break;
                    }
                }
            }
        }
        foreach (AtmosphericAudioSource aas in AAS)
        {
            aas.flightInfo = vMaster.flightInfo;
        }
    }

    //Debug Stuff
    [ContextMenu("DebugFindEngineComponents")]
    public void InitializeMeh()
    {
        vMaster = base.GetComponentInParent<VehicleMaster>();
        foreach (ModuleEngine engine in vMaster.engines)
        {
            Debug.Log("Found ModuleEngine : " + engine);
            foreach (Component component in engine.GetComponentsInChildren<Component>())
            {
                if (!HiddenObjects.Contains(component.gameObject))
                {
                    switch (component.gameObject.name)
                    {
                        case "vgEngineLowPoly":
                            HiddenObjects.Add(component.gameObject);
                            Debug.Log("add vgEngineLowPoly to the HiddenObjects List");
                            break;
                        case "EngineFX":
                            HiddenObjects.Add(component.gameObject);
                            Debug.Log("add EngineFX to the HiddenObjects List");
                            break;
                        case "toggleABAudio":
                            HiddenObjects.Add(component.gameObject);
                            Debug.Log("add toggleABAudio to the HiddenObejcts List");
                            break;
                        case "toggleABAudio (1)":
                            HiddenObjects.Add(component.gameObject);
                            Debug.Log("add toggleABAudio (1) to the HiddenObejects list");
                            break;
                    }
                }
            }
        }
        foreach (GameObject go in HiddenObjects)
        {
            go.SetActive(false);//will need "enabled"
        }
        foreach (GameObject go in NewEngineObjects)
        {
            go.SetActive(true);//will need "!enabled"
        }
        foreach (AtmosphericAudioSource aas in AAS)
        {
            aas.flightInfo = vMaster.flightInfo;
        }
        //should be an if(!enabled) statment around this foreach
        foreach (ModuleEngine engine in vMaster.engines)
        {
            if (engine.name == "fa26-leftEngine")
            {
                engine.engineEffects = EngineFXL;
            }
            if (engine.name == "fa26-rightEngine")
            {
                engine.engineEffects = EngineFXR;
            }
        }

    }
}


