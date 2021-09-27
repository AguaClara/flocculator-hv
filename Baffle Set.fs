FeatureScript 1589;
import(path : "onshape/std/geometry.fs", version : "1589.0");

import(path : "2fa81f50be25609bc956cd5f/9315fcf8489f0c0cc1a06a01/40a6bde79e4081741060af59", version : "24d9ce4bf05b3add5d64a574");
import(path : "ff0f26334dc9ed5e1dbbc027", version : "73ef3187c0c0c31743c8266c");
baffleModule::import(path : "c2f4584cf9d8b1114f7ff5b4", version : "c3aac59959c3a7d35b257c07");
import(path : "c81fce53dede81ef89860aa3/e327e5cbd266d1bdaa593857/b453944163f91ccf1477e3f0", version : "089950d7abdafd6c56066cc3");




export const baffleSetTree =
{
        name : "baffleSet",
        notes : {
            description : "",
            imagelink : "",
            textbooklink : "",
        },
        designers : {
            pre : baffleSetPreDesigner,
            post : baffleSetPostDesigner,
            geometry : baffleSetGeometry,
        },
        params : {
            rep : true,
            ip : "app",
            channelN : [0, 5, 100], //number of baffle sets
            channelB : [0, 8, 200], //center to center distance between channels
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
                    flowfront : "false", //diff
                    lastchannel : "false", //diff
                    tankH : "$.tankH",
                    channelW : "$.channelW",
                    channelL : "$.channelL",
                    FB : "$.FB",
                    baffleT : "$.baffleT",
                    baffleS : "$.baffleS",
                    HL_bod : "$.HL_bod",
                    washerT : "$.washerT",
                    washerOD : "$.washerOD",

                },
            },
        },
    };

export const baffleSetPreDesigner = function(design) returns map
    {



        return design;

    };

export const baffleSetPostDesigner = function(design) returns map
    {

        for (var i = 1; i < design.channelN; i += 1) //for every discrete number from 1 to N
        {


            if ((i == design.channelN) == true) //last channel?
            {
                design.lastchannel = true;
            }
            else
            {
                design.lastchannel = false;
            }


            if ((floor(design.channelN / 2) == ceil(design.channelN / 2)) == true) //if even, rotation of baffle
            {
                //variable for top or bottom insertion (vertical insertion) & rotation
            }

            else
            {
                //same variable as above
            }

            design.baffleSetB = design.channelB * (i - 1);
            //placement horizontally assuming placement at n=1 is 0

            //insert superderive

        }
        return design;

    };


export const baffleSetGeometry = function(context is Context, id is Id, design is map) returns map
    {

        superDerive(context, id, {
                    "partStudio" : { buildFunction : baffleModule::build, configuration : {} } as PartStudioData,
                    location : definition.location
                });

    };

annotation { "Feature Type Name" : "Baffle Set" }
export const baffleSetFeature = defineFeature(function(context is Context, id is Id, definition is map)
    precondition
    {
    }
    {
        treeInstantiatorFeature(context, id, baffleSetTree as InputTree);
    });



//to do list
// - define params
// - define inputs
// - throw in superderive!
// - it will have to be iterative
// - if n = odd, flowfront = true (might not be relevant)
// - if n = N, lastchannel = true
// - could choose to rotate the entire baffle module and place at channelB
// keeps the beginning spacing at baffleS
// flowfront will always be true, that can be deleted from baffle

//questions
// -  if an input below is going to be defined in the next parent, does it still need to be defined in params?
// - how/where do you insert superderive?

