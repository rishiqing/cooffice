'use strict';

process.env.NODE_ENV = 'development-windows';
process.env.NODE_CONFIG_DIR = '../../Common/config';

const cluster = require('cluster');
const configCommon = require('config');
const config = configCommon.get('services.CoAuthoring');
const logger = require('./../../Common/sources/logger');
const co = require('co');
const license = require('./../../Common/sources/license');

if (cluster.isMaster) {
	console.log('cluster.isMaster: ' + cluster.isMaster);
    console.log('cluster.workers: ' + cluster.workers);
	const fs = require('fs');

	let licenseInfo, workersCount = 0, updateTime;

	const readLicense = function*() {
		console.log('----begin to readLicense')
		licenseInfo = yield* license.readLicense();
		console.log('----begin to readLicense: ' + JSON.stringify(licenseInfo))
		workersCount = Math.min(1, licenseInfo.count/*, Math.ceil(numCPUs * cfgWorkerPerCpu)*/);
		console.log('workersCount: ' + workersCount);
	};

	const updateLicense = () => {
		return co(function*() {
			try {
				console.log('========before readLicense: ' + workersCount);
				yield* readLicense();
				console.log('========after readLicense: ' + workersCount);
				// logger.warn('update cluster with %s workers', workersCount);
				// for (let i in cluster.workers) {
				// 	updateLicenseWorker(cluster.workers[i]);
				// }
				// updateWorkers();
			} catch (err) {
				logger.error('updateLicense error:\r\n%s', err.stack);
			}
		});
	};


	cluster.on('fork', (worker) => {
		console.log('----begin to fork');
		updateLicenseWorker(worker);
	});
	cluster.on('exit', (worker, code, signal) => {
		logger.warn('worker %s died (code = %s; signal = %s).', worker.process.pid, code, signal);
		updateWorkers();
	});

	updateLicense();

} else {
	console.log('----is not Master')
}