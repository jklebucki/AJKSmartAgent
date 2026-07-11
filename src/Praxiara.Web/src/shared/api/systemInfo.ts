import { z } from 'zod'

export const systemInfoSchema = z.object({
  name: z.string(),
  version: z.string(),
  status: z.string(),
})

export type SystemInfo = z.infer<typeof systemInfoSchema>
