FeatureScript 1576;
import(path : "onshape/std/geometry.fs", version : "1576.0");
import(path : "2fa81f50be25609bc956cd5f/9315fcf8489f0c0cc1a06a01/40a6bde79e4081741060af59", version : "24d9ce4bf05b3add5d64a574");

import(path : "c0af0d6b5703e7a8fb53f53f/ca3f04e79a55cdd9fedd1ad1/0f7fbd6e5e57d50d045b4ccd", version : "507858454a86a1de9513585e");

export const baffleTree =
{
        name : "baffle",
        notes : {
            description : "",
            imagelink : "",
            textbooklink : "",
        },
        designers : {
            pre : bafflePreDesigner,
            post : bafflePostDesigner,
        },
        params : {
            rep : true,
            ip : "app",
            tankH : [1, 2, 200], //height of tank
            channelW : [0.05, 1, 100], //width of channel
            channelL : [0, 1, 200], //length of channel
            FB : [0, 10, 20], //free board
        },
        children : {
            "plate" : { //check if this is right
                tree : sheetTree,
                inputs : {
                    T : "$.baffleT", //thickness, this will want to be a user input????
                    L : "$.baffleL", //length
                    W : "$.baffleW", //width
                    t : "sheet", //type
                    mat : "PVC", //material
                    ip : "app", //implementation partner
                },
            },
        },
    };

export const bafflePreDesigner = function(design) returns map
    {
        design.baffleT = 1; //user input?
        design.baffleB = 1; //update! and does this go here?
        design.baffleS = design.baffleB - design.baffleT; //spacing between baffles, s = b - t
        design.baffleW = design.channelW; //width of baffle is width of channel
        design.baffleL = design.tankH - design.FB / 2 - design.baffleS; //length = top of tank - (free board/2) - s
        return design;
    };

export const bafflePostDesigner = function(design) returns map
    {
        design.baffleN = design.channelL / design.baffleS; //number of baffles = length of channel / s ; remember, has to be odd or even
        return design;
    };
