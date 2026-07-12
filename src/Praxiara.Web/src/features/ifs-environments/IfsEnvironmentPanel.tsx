import { useEffect, useState } from 'react'
import { useQuery } from '@tanstack/react-query'
import { useTranslation } from 'react-i18next'
import { getIfsEnvironments, getIfsProjectionMetadata } from '../../shared/api/ifsEnvironments'

export function IfsEnvironmentPanel() {
  const { t } = useTranslation()
  const environmentsQuery = useQuery({ queryKey: ['ifs-environments'], queryFn: getIfsEnvironments })
  const [environmentId, setEnvironmentId] = useState('')
  const [projectionName, setProjectionName] = useState('')
  const [metadataState, setMetadataState] = useState<'idle' | 'loading' | 'ready' | 'error'>('idle')

  const selectedEnvironment = environmentsQuery.data?.find((environment) => environment.id === environmentId)

  useEffect(() => {
    const firstEnvironment = environmentsQuery.data?.[0]
    if (firstEnvironment !== undefined && environmentId === '') {
      setEnvironmentId(firstEnvironment.id)
      setProjectionName(firstEnvironment.allowedProjectionNames[0] ?? '')
    }
  }, [environmentId, environmentsQuery.data])

  function handleEnvironmentChange(nextEnvironmentId: string) {
    const nextEnvironment = environmentsQuery.data?.find((environment) => environment.id === nextEnvironmentId)
    setEnvironmentId(nextEnvironmentId)
    setProjectionName(nextEnvironment?.allowedProjectionNames[0] ?? '')
    setMetadataState('idle')
  }

  async function handleMetadataRequest() {
    if (environmentId === '' || projectionName === '') {
      return
    }

    setMetadataState('loading')
    try {
      await getIfsProjectionMetadata(environmentId, projectionName)
      setMetadataState('ready')
    } catch {
      setMetadataState('error')
    }
  }

  return (
    <section className="panel ifs-environment-panel" aria-labelledby="ifs-environments-title">
      <div className="panel-heading">
        <h2 id="ifs-environments-title">{t('ifsEnvironments.title')}</h2>
      </div>
      {environmentsQuery.isLoading && <p className="hint">{t('ifsEnvironments.loading')}</p>}
      {environmentsQuery.isError && <p className="hint" role="status">{t('ifsEnvironments.unavailable')}</p>}
      {environmentsQuery.data !== undefined && environmentsQuery.data.length === 0 && (
        <p className="hint">{t('ifsEnvironments.empty')}</p>
      )}
      {selectedEnvironment !== undefined && (
        <div className="ifs-environment-controls">
          <label>
            <span>{t('ifsEnvironments.environment')}</span>
            <select value={environmentId} onChange={(event) => handleEnvironmentChange(event.target.value)}>
              {environmentsQuery.data?.map((environment) => (
                <option key={environment.id} value={environment.id}>
                  {environment.id} · {environment.environmentKind}
                </option>
              ))}
            </select>
          </label>
          <label>
            <span>{t('ifsEnvironments.projection')}</span>
            <select value={projectionName} onChange={(event) => setProjectionName(event.target.value)}>
              {selectedEnvironment.allowedProjectionNames.map((projection) => (
                <option key={projection} value={projection}>{projection}</option>
              ))}
            </select>
          </label>
          <p className="hint">{selectedEnvironment.baseUri}</p>
          <button type="button" onClick={() => void handleMetadataRequest()} disabled={metadataState === 'loading'}>
            {t('ifsEnvironments.metadata')}
          </button>
          {metadataState === 'ready' && <p className="hint" role="status">{t('ifsEnvironments.metadataReady')}</p>}
          {metadataState === 'error' && <p className="hint" role="status">{t('ifsEnvironments.metadataFailed')}</p>}
        </div>
      )}
    </section>
  )
}
