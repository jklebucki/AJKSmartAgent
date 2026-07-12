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

export async function getIfsEnvironments(): Promise<IfsEnvironment[]> {
  const response = await fetch('/api/v1/ifs/environments', { credentials: 'include' })
  if (!response.ok) {
    throw new Error(`IFS_ENVIRONMENTS_${response.status}`)
  }

  return z.array(ifsEnvironmentSchema).parse(await response.json())
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
