FeatureScript 1605;
import(path : "onshape/std/geometry.fs", version : "1605.0");
import(path : "381423faf2595ecd9f62fd07", version : "e5bc26ac1142286e8acf67d9");


demo::import(path : "16171bc5d51fe4caa0b06c4e", version : "e4bb7d6d1779857924c3d438");

export enum mateName
{
    sed
}

const demoPS = demo::build;

export type DemoParams typecheck canBeDemoParams;



export type FlocHVParams typecheck canBeFlocHVParams;

export predicate canBeFlocHVParams(definition)
{
    annotation { "Name" : "Flow rate (L/s)", "Default" : "30" }
    isAnything(definition.Qm_max);

    annotation { "Name" : "Minimum Temperature (C)" }
    isReal(definition.TEMP_min, { (unitless) : [0, 5, 20] } as RealBoundSpec);

    annotation { "Name" : "Channel length" }
    isLength(definition.L, { (meter) : [3, 7, 40] } as LengthBoundSpec);

    annotation { "Name" : "Outlet water depth" }
    isLength(definition.outletHW, { (meter) : [1, 2, 5] } as LengthBoundSpec);

    annotation { "Name" : "Collision Potential (Gt)" }
    isReal(definition.GT_min, { (unitless) : [20000, 35000, 100000] } as RealBoundSpec);

    annotation { "Name" : "Velocity Gradient (1/s)" }
    isReal(definition.G_bod, { (unitless) : [20, 50, 80] } as RealBoundSpec);
}

annotation { "Feature Type Name" : "Flocculator HV" }
export const flocculatorHV = defineFeature(function(context is Context, id is Id, definition is map)
    precondition
    {
        canBeFlocHVParams(definition);
        canBeDemoParams(definition);
    }
    {
       opMakeDemo(context, id, definition, demoPS);
    });



