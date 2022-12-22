/*
    Some explanation regarding the types.
    The EUP on RnC is linked with "types" so everything fits as good as it can.

    TORSO TYPES:
    Hands
    Elbows
    Shoulders
    ShouldersTorso

    SHIRT TYPES:
    TankTop
    JacketOpen
    ShortSleeves
    LongSleeves
    Armored

    UNDERSHIRT TYPES:
    InsideShirt
    DutyBelt

    PANT TYPES:
    Normal
    AnkleHigh

    BAGS:
    Tie
    Bag

    SHOES:
    High
    Low

    ARMOR:
    KevlarArmor

    ACCESSORIES:
    HipHolster
    Necklace
    TieAccessory

    HATS:
    HatNormal,
    HatHeavy
*/

const FugitiveMaleClothing = [
    {
        clothing: "Shirt",
        type: "TankTop",
        items: [17, 36, 205, 207, 223, 237, 238]
    },
    {
        clothing: "Shirt",
        type: "VestTop",
        items: [25, 21, 40, 120, 176, 290]
    },
    {
        clothing: "Shirt",
        type: "JacketOpen",
        items: [3, 4, 6, 7, 10, 20, 23, 29, 30, 31, 32, 35, 37, 60, 64, 68, 69, 70, 72, 88, 118, 122, 172]
    },
    {
        clothing: "Shirt",
        type: "ShortSleeves",
        items: [0, 1, 9, 16, 22, 33, 34, 39, 44, 56, 73, 83, 93, 94, 105]
    },
    {
        clothing: "Shirt",
        type: "LongSleeves",
        items: [12, 14, 41, 50, 52, 57, 75, 78, 79, 84, 86, 87, 90, 110, 203, 204, 217, 218, 262, 263]
    },
    {
        clothing: "Undershirt",
        type: "InsideShirt",
        items: [0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 16, 17, 18, 21, 22, 29, 30, 31, 32, 34, 35, 15]
    },
    {
        clothing: "Pants",
        type: "Normal",
        items: [0, 1, 3, 4, 5, 6, 7, 8, 9, 10, 12, 13, 15, 16, 17, 19, 20, 22, 23, 24, 25, 26, 27, 28, 29, 42, 43, 45, 46, 47, 48, 49,
                50, 55, 58, 60, 62, 63, 64]
    },
    {
        clothing: "Shoes",
        type: "Low",
        items: [1, 3, 4, 5, 6, 7, 8, 9, 10, 12, 14, 15, 16, 18, 19, 20, 21, 22, 23, 25, 26, 27, 28, 29, 30, 31, 32, 35, 36]
    },
    {
        clothing: "Accessory 1",
        type: "Bag",
        items: [-1, 1, 32, 40, 41, 81, 82]
    },
    {
        clothing: "Accessory 2",
        type: "Necklace",
        items: [-1, 16, 17, 30, 31, 42, 43, 44, 45, 78]
    },
    {
        clothing: "Accessory 2",
        type: "TieAccessory",
        items: [-1, 10, 11, 12, 18, 19, 20, 21, 22, 23, 24, 25, 26, 27]
    },
    {
        clothing: "Handwear",
        type: "Hands",
        items: [4, 20, 35]
    },
    {
        clothing: "Handwear",
        type: "Elbows",
        items: [0, 19, 30]
    },
    {
        clothing: "Handwear",
        type: "Shoulders",
        items: [5, 23, 34]
    },  
    {
        clothing: "Handwear",
        type: "ShouldersTorso",
        items: [15, 29, 40]
    },
    {
        clothing: "Hats",
        type: "HatNormal",
        items: [-1, 2, 4, 172, 5, 6, 7, 12, 14, 20, 21, 26, 28, 44, 45, 54, 56, 76, 96, 151, 152]
    },
]

const FugitiveFemaleClothing = [
    {
        clothing: "Shirt",
        type: "TankTop",
        items: [5, 11, 13, 16, 22, 32, 33, 36, 37]
    },
    {
        clothing: "Shirt",
        type: "VestTop",
        items: [26, 28]
    },
    {
        clothing: "Shirt",
        type: "JacketOpen",
        items: [7, 10, 25, 34, 51, 52, 53, 54, 58, 62, 63, 64, 69, 70, 77]
    },
    {
        clothing: "Shirt",
        type: "ShortSleeves",
        items: [0, 9, 14, 17, 23, 27, 49, 68, 84, 85]
    },
    {
        clothing: "Shirt",
        type: "LongSleeves",
        items: [3, 43, 45, 50, 55, 72, 78, 80]
    },
    {
        clothing: "Undershirt",
        type: "InsideShirt",
        items: [0, 1, 2, 5, 11, 13, 15, 16, 17, 20, 21, 22, 23, 25, 26, 27, 28, 29, 30, 31, 37, 38, 39, 40, 41, 44, 45,
                46, 52, 55, 56, 58, 59, 66, 67]
    },
    {
        clothing: "Pants",
        type: "Normal",
        items: [0, 1, 2, 3, 4, 6, 7, 8, 9, 10, 11, 12, 16, 18, 23, 24, 25, 26, 27, 29, 30, 31, 33, 34, 35, 36, 37, 38, 41, 42]
    },
    {
        clothing: "Shoes",
        type: "Low",
        items: [0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 13, 14, 15, 16, 18, 19, 20, 21, 25, 26, 27, 32, 33, 47]
    },
    {
        clothing: "Accessory 1",
        type: "Bag",
        items: [-1, 1, 32, 40, 41, 81, 82]
    },
    {
        clothing: "Accessory 2",
        type: "Necklace",
        items: [-1, 3, 5, 6, 7, 9, 10, 11, 12, 31, 32, 33, 34, 35]
    },
    {
        clothing: "Accessory 2",
        type: "TieAccessory",
        items: [-1, 19, 20, 21, 22, 23, 26, 27, 28]
    },
    {
        clothing: "Handwear",
        type: "Hands",
        items: [3, 23, 36]
    },
    {
        clothing: "Handwear",
        type: "Elbows",
        items: [0, 20, 33]
    },
    {
        clothing: "Handwear",
        type: "Shoulders",
        items: [4, 24, 37]
    },  
    {
        clothing: "Handwear",
        type: "ShouldersTorso",
        items: [12, 30, 43]
    },
    {
        clothing: "Hats",
        type: "HatNormal",
        items: [-1, 4, 5, 7, 9, 12, 13, 14, 20, 21, 22, 26, 28, 29, 43, 44, 56, 76, 95, 108, 109]
    },
]

const FugitiveLinkedTypes = [
    {
        type: "TankTop",
        accepted:{
            Undershirts: [],
            Pants: ["Normal"],
            Shoes: ["Low"],
            Accessories: ["Necklace"],
            Armor: [], Bags: ["Bag"],
            Torso: ["Shoulders"], Hats: ["HatNormal"]
        }
    },
    {
        type: "VestTop",
        accepted:{
            Undershirts: [],
            Pants: ["Normal"],
            Shoes: ["Low"],
            Accessories: ["Necklace"],
            Armor: [], Bags: ["Bag"],
            Torso: ["ShouldersTorso"], Hats: ["HatNormal"]
        }
    },
    {
        type: "JacketOpen",
        accepted:{
            Undershirts: ["InsideShirt"],
            Pants: ["Normal"],
            Shoes: ["Low"],
            Accessories: ["TieAccessory"],
            Armor: [], Bags: ["Bag"],
            Torso: ["Hands"], Hats: ["HatNormal"]
        }
    },
    {
        type: "ShortSleeves",
        accepted:{
            Undershirts: [],
            Pants: ["Normal"],
            Shoes: ["Low"],
            Accessories: ["Necklace"],
            Armor: ["KevlarArmor"], Bags: ["Bag"],
            Torso: ["Elbows"], Hats: ["HatNormal"]
        }
    },
    {
        type: "LongSleeves",
        accepted:{
            Undershirts: [],
            Pants: ["Normal"],
            Shoes: ["Low"],
            Accessories: ["Necklace"],
            Armor: ["KevlarArmor"], Bags: ["Bag"],
            Torso: ["Hands"], Hats: ["HatNormal"]
        }
    },
];

const FemaleClothing = [
    {
        clothing: 'Shirt',
        type: 'JacketOpen',
        items: [500, 499, 497, 496,495, 494, 491, 489,487, 486, 480]
      },
      {
        clothing: 'Shirt',
        type: 'ShortSleeves',
        items: [ 498, 488, 477, 475, 474 ]
      },
      { 
        clothing: 'Shirt', 
        type: 'LongSleeves', 
        items: [ 503, 493, 476 ] },
      { 
        clothing: 'Shirt', 
        type: 'Armored', 
        items: [ 483 ] 
    },
    {
        clothing: "Undershirt",
        type: "InsideShirt",
        items: [244, 258, 15]
    },
    {
        clothing: "Undershirt",
        type: "DutyBelt",
        items: [245, 243, 240, 239, 238, 15]
    },
    {
        clothing: "Undershirt",
        type: "KevlarUS",
        items: [15]
    },
    { 
        clothing: 'Pants', 
        type: 'Normal', 
        items: [ 176, 174 ] 
    },
    {
      clothing: 'Pants',
      type: 'AnkleHigh',
      items: [ 175, 173, 172, 171 ]
    },
    {
        clothing: "Shoes",
        type: "Low",
        items: [27]
    },
    {
        clothing: "Shoes",
        type: "High",
        items: [25]
    },
    {
        clothing: "Accessory 1",
        type: "Tie",
        items: [-1, 115, 114]
    },
    {
        clothing: "Accessory 2",
        type: "HipHolster",
        items: [-1, 146, 147]
    },
    {
        clothing: "Armor",
        type: "KevlarArmor",
        items: [-1, 58, 59, 62, 66, 69, 76]
    },
    {
        clothing: "Armor",
        type: "HeavyArmor",
        items: [-1, 57, 61, 70, 74, 81]
    },
    {
        clothing: "Handwear",
        type: "Hands",
        items: [3, 26, 36]
    },
    {
        clothing: "Handwear",
        type: "Elbows",
        items: [0, 20, 33]
    },
    {
        clothing: "Hats",
        type: "HatNormal",
        items: [-1, 45, 141]
    },
    {
        clothing: "Hats",
        type: "HatHeavy",
        items: [-1, 140]
    },
]

const MaleClothing = [
    {
        clothing: 'Shirt',        
        type: 'JacketOpen',       
        items: [497, 491, 484, 482, 473,468, 467, 466, 463, 462,460, 459, 457, 456, 455,454, 453, 451]
      },
      {
        clothing: 'Shirt',        
        type: 'ShortSleeves',
        items: [499, 498, 486,477, 472, 449,447, 446]
      },
      {
        clothing: 'Shirt',
        type: 'LongSleeves',
        items: [ 488, 487, 476, 471, 448 ]
      },
      {
        clothing: 'Shirt',
        type: 'Armored',
        items: [ 493, 492, 489, 480, 479 ]
      },
      {
        clothing: 'Undershirt',
        type: 'InsideShirt',
        items: [244, 243, 234, 233,231, 230, 225, 224,210, 209, 15]
      },
      {
        clothing: 'Undershirt',
        type: 'DutyBelt',
        items: [ 229, 228, 227, 226, 221, 220, 219, 218, 214, 213,212, 208, 207, 204, 203,201, 200, 199, 15]
      },
    { 
        clothing: 'Undershirt', 
        type: 'KevlarUS', 
        items: [ -1, 216, 222, 15] 
    },
    { 
        clothing: 'Pants', 
        type: 'Normal', 
        items: [ 167, 164 ] 
    },
    {
      clothing: 'Pants',
      type: 'AnkleHigh',
      items: [ 175, 166, 165, 163 ]
    },
    {
        clothing: "Shoes",
        type: "Low",
        items: [127]
    },
    {
        clothing: "Shoes",
        type: "High",
        items: [126]
    },
    { 
        clothing: 'Accessory 1', 
        type: 'Tie', 
        items: [ -1, 128, 127 ] 
    },
    {
      clothing: 'Accessory 2',
      type: 'HipHolster',
      items: [ -1, 183, 184 ]
    },
    {
        clothing: "Armor",
        type: "KevlarArmor",
        items: [-1, 63, 68, 70, 71, 77, 80, 82]
    },
    {
        clothing: "Armor",
        type: "HeavyArmor",
        items: [-1, 62, 75]
    },
    {
        clothing: "Handwear",
        type: "Hands",
        items: [4, 22, 44]
    },
    {
        clothing: "Handwear",
        type: "Elbows",
        items: [0, 11, 19, 41]
    },
    {
        clothing: "Hats",
        type: "HatNormal",
        items: [-1, 46, 142]
    },
    {
        clothing: "Hats",
        type: "HatHeavy",
        items: [-1, 141]
    },
]

const LinkedTypes = [
    {
        type: "JacketOpen",
        accepted:{
            Undershirts: ["InsideShirt"],
            Pants: ["Normal"],
            Shoes: ["Low"],
            Accessories: ["HipHolster"],
            Armor: [], Bags: ["Tie"],
            Torso: ["Hands"], Hats: ["HatNormal"]
        }
    },
    {
        type: "ShortSleeves",
        accepted:{
            Undershirts: ["DutyBelt", "KevlarUS"],
            Pants: ["Normal"],
            Shoes: ["Low"],
            Accessories: ["HipHolster"],
            Armor: ["KevlarArmor"], Bags: [],
            Torso: ["Elbows"], Hats: ["HatNormal"]
        }
    },
    {
        type: "LongSleeves",
        accepted:{
            Undershirts: ["DutyBelt", "KevlarUS"],
            Pants: ["Normal"],
            Shoes: ["Low"],
            Accessories: ["HipHolster"],
            Armor: ["KevlarArmor"], Bags: [],
            Torso: ["Hands"], Hats: ["HatNormal"]
        }
    },
    {
        type: "Armored",
        accepted:{
            Undershirts: ["DutyBelt"],
            Pants: ["AnkleHigh"],
            Shoes: ["High"],
            Accessories: ["HipHolster"],
            Armor: ["KevlarArmor", "HeavyArmor"], Bags: [],
            Torso: ["Hands"], Hats: ["HatHeavy"]
        }
    }
];