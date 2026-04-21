<template>
  <footer class="app-footer">
    <div class="footer-copyright">
      © 2024 Poängtavlan Editorial
    </div>
    <div class="footer-links">
      <a href="#" class="footer-link">Support</a>
      <a href="#" class="footer-link">Regler</a>
      <a href="#" class="footer-link">Integritet</a>
    </div>
  </footer>
</template>

<style scoped>
.app-footer {
  background-color: var(--color-background);
  border-top: 1px solid var(--color-outline-variant);
  display: flex;
  flex-direction: column;
  justify-content: space-between;
  align-items: center;
  padding: 32px 48px;
  width: 100%;
  font-family: 'Manrope', sans-serif;
  font-size: 12px;
  text-transform: uppercase;
  letter-spacing: 0.15em;
}

.footer-copyright {
  color: var(--color-on-surface-variant);
  margin-bottom: 16px;
}

.footer-links {
  display: flex;
  gap: 32px;
}

.footer-link {
  color: var(--color-on-surface-variant);
  text-decoration: none;
  transition: color 200ms ease-out;
}

.footer-link:hover {
  color: var(--color-primary);
}

@media (min-width: 768px) {
  .app-footer {
    flex-direction: row;
    padding: 32px 48px;
  }
  
  .footer-copyright {
    margin-bottom: 0;
  }
}
</style>
