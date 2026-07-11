import { useTranslation } from 'react-i18next'

export function BrowserPanel() {
  const { t } = useTranslation()

  return (
    <section className="panel browser-panel" aria-labelledby="browser-title">
      <div className="panel-heading">
        <h2 id="browser-title">{t('browser.title')}</h2>
        <button type="button" className="secondary-button" disabled>
          {t('browser.takeover')}
        </button>
      </div>
      <div className="browser-frame">
        <div className="browser-toolbar" aria-hidden="true">
          <span />
          <span />
          <span />
          <div className="address-bar" />
        </div>
        <div className="empty-state">
          <strong>{t('browser.emptyTitle')}</strong>
          <p>{t('browser.emptyBody')}</p>
        </div>
      </div>
    </section>
  )
}
