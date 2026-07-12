import { z } from 'zod'

const ifsEnvironmentSchema = z.object({
  id: z.string(),
  baseUri: z.string().url(),
  tenant: z.string(),
  locale: z.string(),
  environmentKind: z.string(),
  allowedProjectionNames: z.array(z.string()),
  authenticationMode: z.string(),
  tokenEndpoint: z.string().url().nullable(),
  clientId: z.string().nullable(),
  isSecretReferenceConfigured: z.boolean(),
})

export type IfsEnvironment = z.infer<typeof ifsEnvironmentSchema>

const authenticationSessionSchema = z.object({
  userName: z.string(),
  roles: z.array(z.string()),
})

export type AuthenticationSession = z.infer<typeof authenticationSessionSchema>

export type IfsEnvironmentInput = {
  id: string
  baseUri: string
  tenant: string
  locale: string
  environmentKind: string
  allowedProjectionNames: string[]
  authenticationMode: string
  secretFilePath?: string
  tokenEndpoint?: string | null
  clientId?: string | null
}

export async function getIfsEnvironments(): Promise<IfsEnvironment[]> {
  const response = await fetch('/api/v1/ifs/environments', { credentials: 'include' })
  if (!response.ok) {
    throw new Error(`IFS_ENVIRONMENTS_${response.status}`)
  }

  return z.array(ifsEnvironmentSchema).parse(await response.json())
}

export async function createIfsEnvironment(input: IfsEnvironmentInput): Promise<IfsEnvironment> {
  const antiforgeryToken = await getAntiforgeryToken()
  const response = await fetch('/api/v1/ifs/environments', {
    method: 'POST',
    credentials: 'include',
    headers: { 'Content-Type': 'application/json', 'X-CSRF-TOKEN': antiforgeryToken },
    body: JSON.stringify(input),
  })

  return parseIfsEnvironmentResponse(response, 'IFS_ENVIRONMENT_CREATE')
}

export async function updateIfsEnvironment(id: string, input: Omit<IfsEnvironmentInput, 'id'>): Promise<IfsEnvironment> {
  const antiforgeryToken = await getAntiforgeryToken()
  const response = await fetch(`/api/v1/ifs/environments/${encodeURIComponent(id)}`, {
    method: 'PUT',
    credentials: 'include',
    headers: { 'Content-Type': 'application/json', 'X-CSRF-TOKEN': antiforgeryToken },
    body: JSON.stringify(input),
  })

  return parseIfsEnvironmentResponse(response, 'IFS_ENVIRONMENT_UPDATE')
}

export async function deleteIfsEnvironment(id: string): Promise<void> {
  const antiforgeryToken = await getAntiforgeryToken()
  const response = await fetch(`/api/v1/ifs/environments/${encodeURIComponent(id)}`, {
    method: 'DELETE',
    credentials: 'include',
    headers: { 'X-CSRF-TOKEN': antiforgeryToken },
  })
  if (!response.ok) {
    throw new Error(`IFS_ENVIRONMENT_DELETE_${response.status}`)
  }
}

export async function getAuthenticationSession(): Promise<AuthenticationSession | null> {
  const response = await fetch('/api/v1/auth/session', { credentials: 'include' })
  if (response.status === 401) {
    return null
  }
  if (!response.ok) {
    throw new Error(`PRAXIARA_AUTH_SESSION_${response.status}`)
  }

  return authenticationSessionSchema.parse(await response.json())
}

export async function logout(): Promise<void> {
  const antiforgeryToken = await getAntiforgeryToken()
  const response = await fetch('/api/v1/auth/logout', {
    method: 'POST',
    credentials: 'include',
    headers: { 'X-CSRF-TOKEN': antiforgeryToken },
  })
  if (!response.ok) {
    throw new Error(`PRAXIARA_LOGOUT_${response.status}`)
  }
}

export async function getIfsProjectionMetadata(environmentId: string, projectionName: string): Promise<string> {
  const response = await fetch(
    `/api/v1/ifs/environments/${encodeURIComponent(environmentId)}/projections/${encodeURIComponent(projectionName)}/metadata`,
    { credentials: 'include' },
  )
  if (!response.ok) {
    throw new Error(`IFS_METADATA_${response.status}`)
  }

  return response.text()
}

async function parseIfsEnvironmentResponse(response: Response, errorPrefix: string): Promise<IfsEnvironment> {
  if (!response.ok) {
    throw new Error(`${errorPrefix}_${response.status}`)
  }

  return ifsEnvironmentSchema.parse(await response.json())
}

async function getAntiforgeryToken(): Promise<string> {
  const response = await fetch('/api/v1/auth/antiforgery', { credentials: 'include' })
  if (!response.ok) {
    throw new Error(`PRAXIARA_AUTH_${response.status}`)
  }

  return z.object({ requestToken: z.string().min(1) }).parse(await response.json()).requestToken
}
