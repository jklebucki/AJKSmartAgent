import { useTranslation } from 'react-i18next'

export function ApprovalPanel() {
  const { t } = useTranslation()

  return (
    <aside className="panel approval-panel" aria-labelledby="approval-title">
      <h2 id="approval-title">{t('approval.title')}</h2>
      <div className="empty-approval">{t('approval.empty')}</div>
    </aside>
  )
}
