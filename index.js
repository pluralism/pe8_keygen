let getRandomNumber = (min, max) => {
    return Math.floor(min + Math.random() * (max - min + 1));
};

let reverseString = (str) => {
    return str.split("").reverse().join("");
};

let getSecretMatrix = () => {
    return [22, 12, 16, 33, 28, 5, 36, 10, 20, 24, 8, 32, 26, 35, 15, 14, 17, 2, 11, 6, 31, 34, 3, 19, 13, 27, 25, 18, 29, 9, 30, 23, 4, 21, 7, 1]
}

String.prototype.replaceAt = function(index, replacement) {
    return this.substr(0, index) + replacement + this.substr(index + 1);
}

let calculateKeySum = (key) => {
    let sum = 0;
    let reversedKey = reverseString(key);

    let multiplier = 1;
    for(let i = 0; i < reversedKey.length; i++, multiplier++) {
        sum += parseInt(reversedKey[i], 10) * (multiplier + 1);
    }

    return sum;
}

let calculateSecretNumber = (sum) => {
    sum %= 11;
    return sum < 2 ? 0 : 11 - sum;
}

let generateKey = () => {
    let codeKey = "";
    let generatedRandomNumber = 0;

    for(let i = 0; i < 10; i++) {
        generatedRandomNumber = getRandomNumber(2, 9);
        codeKey += generatedRandomNumber;
    }

    let keySum = calculateKeySum(codeKey);
    codeKey += calculateSecretNumber(keySum);

    for(let i = 0; i < 11; i++) {
        generatedRandomNumber = getRandomNumber(2, 9);
        codeKey += generatedRandomNumber;
    }

    keySum = calculateKeySum(codeKey);
    codeKey += calculateSecretNumber(codeKey);

    for(let i = 0; i < 4; i++) {
        generatedRandomNumber = getRandomNumber(2, 9);
        codeKey += generatedRandomNumber;
    }

    codeKey += "24032030";
    keySum = calculateKeySum(codeKey);
    codeKey += calculateSecretNumber(codeKey);
    console.log(codeKey);

    let secretMatrix = getSecretMatrix();
    let scrambledKey = "";
    for(let i = 0; i < secretMatrix.length; i++) {
        scrambledKey += " ";
    }

    for(let i = 0; i < secretMatrix.length; i++) {
        scrambledKey = scrambledKey.replaceAt(i, codeKey[secretMatrix[i] - 1]);
    }
    
    let finalKey = "";
    for(let i = 0; i < 10; i++) {
        finalKey += scrambledKey[i];
    }

    finalKey += "-";

    for(let i = 10; i < 18; i++) {
        finalKey += scrambledKey[i];
    }

    finalKey += "-";

    for(let i = 18; i < 28; i++) {
        finalKey += scrambledKey[i];
    }

    finalKey += "-";

    for(let i = 28; i < scrambledKey.length; i++) {
        finalKey += scrambledKey[i];
    }
    
    return finalKey;
};

console.log(generateKey());