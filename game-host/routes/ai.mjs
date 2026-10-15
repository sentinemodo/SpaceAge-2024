import { json, readBody } from '../lib/http.mjs';
import { effectiveFactionId, requireSession } from '../lib/auth.mjs';
import { executeAiQuery, regenerateStory } from '../lib/ai-session.mjs';
import {
  listPersonas,
  readPersona,
  readStory,
  savePersona,
  saveStory,
} from '../lib/persona-story.mjs';
import { sessionRunId } from '../lib/session-view.mjs';

export async function handleAi(req, res, url) {
  if (req.method === 'GET' && url.pathname === '/api/session/personas') {
    const session = requireSession(req, res);
    if (!session) return true;
    json(res, 200, { personas: listPersonas(sessionRunId(session)) });
    return true;
  }

  if (req.method === 'GET' && url.pathname === '/api/session/persona') {
    const session = requireSession(req, res);
    if (!session) return true;
    const personaId = url.searchParams.get('id');
    if (!personaId) {
      json(res, 400, { error: 'missing id' });
      return true;
    }
    try {
      json(res, 200, { personaId, text: readPersona(sessionRunId(session), personaId) });
    } catch (err) {
      json(res, 400, { error: String(err.message || err) });
    }
    return true;
  }

  if (req.method === 'PUT' && url.pathname === '/api/session/persona') {
    const session = requireSession(req, res);
    if (!session) return true;
    if (!session.admin) {
      json(res, 403, { error: 'admin/gm only' });
      return true;
    }
    const body = JSON.parse(await readBody(req));
    if (!body.personaId || body.text == null) {
      json(res, 400, { error: 'personaId and text required' });
      return true;
    }
    try {
      savePersona(sessionRunId(session), body.personaId, body.text);
      json(res, 200, { ok: true });
    } catch (err) {
      json(res, 400, { error: String(err.message || err) });
    }
    return true;
  }

  if (req.method === 'GET' && url.pathname === '/api/session/story') {
    const session = requireSession(req, res);
    if (!session) return true;
    const text = readStory(sessionRunId(session), effectiveFactionId(session));
    json(res, 200, { text });
    return true;
  }

  if (req.method === 'PUT' && url.pathname === '/api/session/story') {
    const session = requireSession(req, res);
    if (!session) return true;
    const body = await readBody(req);
    saveStory(sessionRunId(session), effectiveFactionId(session), body);
    json(res, 200, { ok: true });
    return true;
  }

  if (req.method === 'POST' && url.pathname === '/api/session/ai/query') {
    const session = requireSession(req, res);
    if (!session) return true;
    const body = JSON.parse(await readBody(req));
    if (!body.prompt?.trim()) {
      json(res, 400, { error: 'missing prompt' });
      return true;
    }
    try {
      const result = await executeAiQuery({
        runId: sessionRunId(session),
        factionId: effectiveFactionId(session),
        prompt: body.prompt.trim(),
        includeStory: !!body.includeStory,
      });
      json(res, 200, result);
    } catch (err) {
      json(res, 500, { ok: false, error: String(err.message || err) });
    }
    return true;
  }

  if (req.method === 'POST' && url.pathname === '/api/session/ai/draft-story') {
    const session = requireSession(req, res);
    if (!session) return true;
    const body = JSON.parse(await readBody(req));
    if (!body.personaId) {
      json(res, 400, { error: 'missing personaId' });
      return true;
    }
    try {
      const result = await regenerateStory({
        runId: sessionRunId(session),
        factionId: effectiveFactionId(session),
        personaId: body.personaId,
      });
      json(res, 200, result);
    } catch (err) {
      json(res, 500, { ok: false, error: String(err.message || err) });
    }
    return true;
  }

  return false;
}
