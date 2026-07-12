import { useEffect, useState, type FormEvent } from 'react'
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { useTranslation } from 'react-i18next'
import {
  createIfsEnvironment,
  deleteIfsEnvironment,
  getIfsEnvironments,
  getIfsProjectionMetadata,
  type IfsEnvironment,
  type IfsEnvironmentInput,
  updateIfsEnvironment,
} from '../../shared/api/ifsEnvironments'

type FormState = {
  id: string
  baseUri: string
  tenant: string
  locale: string
  environmentKind: string
  allowedProjectionNames: string
  authenticationMode: string
  secretFilePath: string
  tokenEndpoint: string
  clientId: string
}

const emptyForm: FormState = {
  id: '', baseUri: '', tenant: '', locale: 'pl-PL', environmentKind: 'Test', allowedProjectionNames: '',
  authenticationMode: 'BearerTokenFile', secretFilePath: '', tokenEndpoint: '', clientId: '',
}

function toFormState(environment: IfsEnvironment): FormState {
  return {
    id: environment.id,
    baseUri: environment.baseUri,
    tenant: environment.tenant,
    locale: environment.locale,
    environmentKind: environment.environmentKind,
    allowedProjectionNames: environment.allowedProjectionNames.join('\n'),
    authenticationMode: environment.authenticationMode,
    secretFilePath: '',
    tokenEndpoint: environment.tokenEndpoint ?? '',
    clientId: environment.clientId ?? '',
  }
}

function toInput(form: FormState): IfsEnvironmentInput {
  return {
    id: form.id.trim(),
    baseUri: form.baseUri.trim(),
    tenant: form.tenant.trim(),
    locale: form.locale.trim(),
    environmentKind: form.environmentKind,
    allowedProjectionNames: form.allowedProjectionNames.split(/[\n,]/).map((value) => value.trim()).filter(Boolean),
    authenticationMode: form.authenticationMode,
    ...(form.secretFilePath.trim() === '' ? {} : { secretFilePath: form.secretFilePath.trim() }),
    ...(form.tokenEndpoint.trim() === '' ? { tokenEndpoint: null } : { tokenEndpoint: form.tokenEndpoint.trim() }),
    ...(form.clientId.trim() === '' ? { clientId: null } : { clientId: form.clientId.trim() }),
  }
}

export function IfsEndpointManagementPage() {
  const { t } = useTranslation()
  const queryClient = useQueryClient()
  const environmentsQuery = useQuery({ queryKey: ['ifs-environments'], queryFn: getIfsEnvironments })
  const [form, setForm] = useState<FormState>(emptyForm)
  const [editingId, setEditingId] = useState<string | null>(null)
  const [selectedProjection, setSelectedProjection] = useState('')
  const [metadataState, setMetadataState] = useState<'idle' | 'loading' | 'ready' | 'error'>('idle')
  const [operationError, setOperationError] = useState('')
  const selectedEnvironment = environmentsQuery.data?.find((environment) => environment.id === editingId)

  const createMutation = useMutation({
    mutationFn: createIfsEnvironment,
    onSuccess: () => void queryClient.invalidateQueries({ queryKey: ['ifs-environments'] }),
    onError: () => setOperationError(t('ifsManagement.saveFailed')),
  })
  const updateMutation = useMutation({
    mutationFn: ({ id, input }: { id: string, input: Omit<IfsEnvironmentInput, 'id'> }) => updateIfsEnvironment(id, input),
    onSuccess: () => void queryClient.invalidateQueries({ queryKey: ['ifs-environments'] }),
    onError: () => setOperationError(t('ifsManagement.saveFailed')),
  })
  const deleteMutation = useMutation({
    mutationFn: deleteIfsEnvironment,
    onSuccess: () => {
      void queryClient.invalidateQueries({ queryKey: ['ifs-environments'] })
      resetForm()
    },
    onError: () => setOperationError(t('ifsManagement.deleteFailed')),
  })

  useEffect(() => {
    if (selectedEnvironment !== undefined && selectedProjection === '') {
      setSelectedProjection(selectedEnvironment.allowedProjectionNames[0] ?? '')
    }
  }, [selectedEnvironment, selectedProjection])

  function resetForm() {
    setForm(emptyForm)
    setEditingId(null)
    setSelectedProjection('')
    setMetadataState('idle')
    setOperationError('')
  }

  function beginEditing(environment: IfsEnvironment) {
    setForm(toFormState(environment))
    setEditingId(environment.id)
    setSelectedProjection(environment.allowedProjectionNames[0] ?? '')
    setMetadataState('idle')
    setOperationError('')
  }

  function updateField(field: keyof FormState, value: string) {
    setForm((current) => ({ ...current, [field]: value }))
  }

  async function save(event: FormEvent<HTMLFormElement>) {
    event.preventDefault()
    setOperationError('')
    const input = toInput(form)
    if (editingId === null) {
      await createMutation.mutateAsync(input)
      return
    }

    const { id: _, ...updateInput } = input
    await updateMutation.mutateAsync({ id: editingId, input: updateInput })
  }

  async function verifyMetadata() {
    if (editingId === null || selectedProjection === '') {
      return
    }
    setMetadataState('loading')
    try {
      await getIfsProjectionMetadata(editingId, selectedProjection)
      setMetadataState('ready')
    } catch {
      setMetadataState('error')
    }
  }

  const isSaving = createMutation.isPending || updateMutation.isPending

  return (
    <main className="ifs-management-page" aria-labelledby="ifs-management-title">
      <div className="page-heading">
        <div>
          <span className="eyebrow">IFS / OData</span>
          <h2 id="ifs-management-title">{t('ifsManagement.title')}</h2>
          <p>{t('ifsManagement.description')}</p>
        </div>
        <button className="secondary-button" type="button" onClick={resetForm}>{t('ifsManagement.newEnvironment')}</button>
      </div>

      <div className="ifs-management-grid">
        <section className="panel" aria-labelledby="ifs-environment-list-title">
          <h3 id="ifs-environment-list-title">{t('ifsManagement.configured')}</h3>
          {environmentsQuery.isLoading && <p className="hint">{t('ifsEnvironments.loading')}</p>}
          {environmentsQuery.isError && <p className="hint" role="status">{t('ifsEnvironments.unavailable')}</p>}
          {environmentsQuery.data?.length === 0 && <p className="hint">{t('ifsEnvironments.empty')}</p>}
          <ul className="endpoint-list">
            {environmentsQuery.data?.map((environment) => (
              <li key={environment.id}>
                <button className={editingId === environment.id ? 'endpoint-list-item is-selected' : 'endpoint-list-item'} type="button" onClick={() => beginEditing(environment)}>
                  <strong>{environment.id}</strong>
                  <span>{environment.environmentKind} · {environment.baseUri}</span>
                </button>
              </li>
            ))}
          </ul>
        </section>

        <section className="panel" aria-labelledby="ifs-editor-title">
          <h3 id="ifs-editor-title">{editingId === null ? t('ifsManagement.create') : t('ifsManagement.edit')}</h3>
          <form className="ifs-management-form" onSubmit={(event) => void save(event)}>
            <label><span>{t('ifsManagement.id')}</span><input value={form.id} onChange={(event) => updateField('id', event.target.value)} disabled={editingId !== null} required /></label>
            <label><span>{t('ifsManagement.baseUri')}</span><input type="url" value={form.baseUri} onChange={(event) => updateField('baseUri', event.target.value)} required /></label>
            <div className="form-row">
              <label><span>{t('ifsManagement.tenant')}</span><input value={form.tenant} onChange={(event) => updateField('tenant', event.target.value)} required /></label>
              <label><span>{t('ifsManagement.locale')}</span><input value={form.locale} onChange={(event) => updateField('locale', event.target.value)} required /></label>
            </div>
            <div className="form-row">
              <label><span>{t('ifsManagement.kind')}</span><select value={form.environmentKind} onChange={(event) => updateField('environmentKind', event.target.value)}><option>Development</option><option>Test</option><option>Acceptance</option><option>Production</option></select></label>
              <label><span>{t('ifsManagement.authentication')}</span><select value={form.authenticationMode} onChange={(event) => updateField('authenticationMode', event.target.value)}><option>BearerTokenFile</option><option>ClientCredentials</option></select></label>
            </div>
            <label><span>{t('ifsManagement.projections')}</span><textarea value={form.allowedProjectionNames} onChange={(event) => updateField('allowedProjectionNames', event.target.value)} required /><small>{t('ifsManagement.projectionsHint')}</small></label>
            <label><span>{t('ifsManagement.secretFile')}</span><input value={form.secretFilePath} onChange={(event) => updateField('secretFilePath', event.target.value)} />{editingId !== null && <small>{t('ifsManagement.secretFileHint')}</small>}</label>
            {form.authenticationMode === 'ClientCredentials' && <>
              <label><span>{t('ifsManagement.tokenEndpoint')}</span><input type="url" value={form.tokenEndpoint} onChange={(event) => updateField('tokenEndpoint', event.target.value)} required /></label>
              <label><span>{t('ifsManagement.clientId')}</span><input value={form.clientId} onChange={(event) => updateField('clientId', event.target.value)} required /></label>
            </>}
            {operationError !== '' && <p className="form-error" role="status">{operationError}</p>}
            <div className="form-actions">
              <button type="submit" disabled={isSaving}>{editingId === null ? t('ifsManagement.create') : t('ifsManagement.save')}</button>
              {editingId !== null && <button className="secondary-button" type="button" disabled={deleteMutation.isPending} onClick={() => void deleteMutation.mutateAsync(editingId)}>{t('ifsManagement.delete')}</button>}
            </div>
          </form>
        </section>

        <section className="panel" aria-labelledby="ifs-metadata-title">
          <h3 id="ifs-metadata-title">{t('ifsManagement.metadataTitle')}</h3>
          {selectedEnvironment === undefined ? <p className="hint">{t('ifsManagement.metadataSelect')}</p> : <div className="ifs-environment-controls">
            <p className="hint">{selectedEnvironment.baseUri}</p>
            <label><span>{t('ifsEnvironments.projection')}</span><select value={selectedProjection} onChange={(event) => setSelectedProjection(event.target.value)}>{selectedEnvironment.allowedProjectionNames.map((projection) => <option key={projection} value={projection}>{projection}</option>)}</select></label>
            <button type="button" onClick={() => void verifyMetadata()} disabled={metadataState === 'loading' || selectedProjection === ''}>{t('ifsEnvironments.metadata')}</button>
            {metadataState === 'ready' && <p className="hint" role="status">{t('ifsEnvironments.metadataReady')}</p>}
            {metadataState === 'error' && <p className="hint" role="status">{t('ifsEnvironments.metadataFailed')}</p>}
          </div>}
        </section>
      </div>
    </main>
  )
}
