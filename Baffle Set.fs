FeatureScript 1589;
import(path : "onshape/std/geometry.fs", version : "1589.0");

import(path : "2fa81f50be25609bc956cd5f/9315fcf8489f0c0cc1a06a01/40a6bde79e4081741060af59", version : "24d9ce4bf05b3add5d64a574");
import(path : "c2f4584cf9d8b1114f7ff5b4", version : "c3aac59959c3a7d35b257c07");


export const bafflesetTree =
{
        name : "baffleset",
        notes : {
            description : "",
            imagelink : "",
            textbooklink : "",
        },
        designers : {
            pre : bafflesetPreDesigner,
            post : bafflesetPostDesigner,
        },
        params : {
            rep : true,
            ip : "app",
            N: [0, 5, 100], //number of baffle sets
            flowfront : "false",
            lastchannel : "false",
            tankH : [1, 2, 200], 
            channelW : [0.05, 0.5, 100], 
            channelL : [0, 7, 200], 
            FB : [0, 0.1, 1], 
            baffleT : [0, 0.0008, 2], 
            baffleS : [0.01, 0.1, 10], 
            HL_bod : [0, 0.4, 1], 
            washerT : [0.001, 0.003175, 0.2], 
            washerOD : [0.0002, 1, 3],

        },
        children : {
            "baffle" : {
                tree : baffleTree,
                inputs : {

                    rep : "$.rep",
                    ip : "$.ip",
                    flowfront : "false",
                    lastchannel : "false",
                    tankH : "$.tankH",
                    channelW : "$.channelW",
                    channelL : "$.channelL",
                    FB : "$.FB",
                    baffleT :"$.baffleT",
                    baffleS : "$.baffleS",
                    HL_bod : "$.HL_bod,
                    washerT : "$.washerT",
                    washerOD : "$.washerOD",

                },
            },
        },
    };

export const bafflesetPreDesigner = function(design) returns map
    {

        return design;

    };

export const bafflesetPostDesigner = function(design) returns map
    {

        return design;

    };


annotation { "Feature Type Name" : "Baffle Set" }
export const bafflesetFeature = defineFeature(function(context is Context, id is Id, definition is map)
    precondition
    {
    }
    {
        treeInstantiatorFeature(context, id, bafflesetTree as InputTree);
    });

    
    
    //to do list
    // - define params
    // - define inputs
    
    //questions
    // -  if an input below is going to be defined in the next parent, does it still need to be defined in params?
    //  - where in the code in superderive inserted?
    
