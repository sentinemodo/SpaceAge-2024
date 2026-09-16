import assert from 'node:assert/strict';
import { describe, it } from 'node:test';
import {
  gpuAlternationList,
  isActivePodStatus,
  isGpuCapacityError,
  isRunPodAuthError,
  pickManagedPod,
  probeOllamaReady,
} from '../lib/runpod.mjs';

describe('runpod gpu helpers', () => {
  it('detects gpu capacity errors', () => {
    assert.equal(
      isGpuCapacityError(new Error('RunPod 400: There are no longer any instances available')),
      true,
    );
    assert.equal(isGpuCapacityError(new Error('RunPod 500: internal')), false);
  });

  it('detects auth errors', () => {
    assert.equal(isRunPodAuthError(new Error('RunPod 400: Unauthorized')), true);
    assert.equal(isRunPodAuthError(new Error('RunPod 401: Invalid API key')), true);
    assert.equal(isRunPodAuthError(new Error('RunPod 400: no instances')), false);
  });

  it('alternates 4090 and 5090 by default', () => {
    assert.deepEqual(gpuAlternationList(undefined), [
      'NVIDIA GeForce RTX 4090',
      'NVIDIA GeForce RTX 5090',
    ]);
    assert.deepEqual(gpuAlternationList('NVIDIA GeForce RTX 4090'), [
      'NVIDIA GeForce RTX 4090',
      'NVIDIA GeForce RTX 5090',
    ]);
  });

  it('uses custom gpu when not in alternate list', () => {
    assert.deepEqual(gpuAlternationList('NVIDIA A100'), ['NVIDIA A100']);
  });

  it('probeOllamaReady returns false for invalid host', async () => {
    assert.equal(await probeOllamaReady(null), false);
    assert.equal(await probeOllamaReady('http://127.0.0.1:1'), false);
  });

  it('picks newest active spaceage pod', () => {
    const pods = [
      { id: 'old', name: 'spaceage-1', status: 'EXITED', createdAt: '2026-01-02T00:00:00Z' },
      { id: 'new', name: 'spaceage-2', status: 'RUNNING', createdAt: '2026-01-03T00:00:00Z' },
      { id: 'other', name: 'unrelated', status: 'RUNNING', createdAt: '2026-01-04T00:00:00Z' },
    ];
    assert.equal(pickManagedPod(pods, { volumeId: 'vol1' })?.id, 'new');
    assert.equal(isActivePodStatus('RUNNING'), true);
    assert.equal(isActivePodStatus('EXITED'), false);
  });
});
