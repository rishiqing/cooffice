const crypto = require('crypto');
const fs = require('fs');

const signMethod = 'RSA-SHA1';
const privateKey = fs.readFileSync('./licensetest/private');
const publicKey = fs.readFileSync('./licensetest/public.key');
// const publicKey = '-----BEGIN PUBLIC KEY-----\nMIGfMA0GCSqGSIb3DQEBAQUAA4GNADCBiQKBgQDRhGF7X4A0ZVlEg594WmODVVUI\niiPQs04aLmvfg8SborHss5gQXu0aIdUT6nb5rTh5hD2yfpF2WIW6M8z0WxRhwicg\nXwi80H1aLPf6lEPPLvN29EhQNjBpkFkAJUbS8uuhJEeKw0cE49g80eBBF4BCqSL6\nPFQbP9/rByxdxEoAIQIDAQAB\n-----END PUBLIC KEY-----\n';

const licenseFilePath = './licensetest/license.lic';
const licenseProps = {
		end_date: '2200-01-01',
		trial: false,
		mode: 0, //  None: 0, Trial: 1, Developer: 2
		version: 2,
		process: 999999, //  cpu数量
		light: false,  //  轻量级模式，不能协同
		branding: true,  //  是否商标可以个性化定制？
		connections: 999999,
		users_count: 999999,
		users_expire: 1,
	};

function sign (privateKey, content) {
    const sign = crypto.createSign(signMethod);
    sign.write(content);
    sign.end();
    return sign.sign(privateKey, 'hex');
}
function verify (publicKey, content, signature) {
	const verify = crypto.createVerify(signMethod);
    verify.update(content);
    return verify.verify(publicKey, signature, 'hex');
}
function generateLicense(privateKey, licenseObj) {
	const sg = sign(privateKey, JSON.stringify(licenseObj));
	licenseObj.signature = sg;
	fs.writeFileSync(licenseFilePath, JSON.stringify(licenseObj));
	console.log('>>license file generated successfully: ' + licenseFilePath);
}

generateLicense(privateKey, licenseProps);


// const signature = sign(privateKey, content);
// console.log(signature);
// const verifyResult = verify(publicKey, content, signature);
// console.log('verify result: ' + verifyResult);

// console.log('result:::: ' + result);