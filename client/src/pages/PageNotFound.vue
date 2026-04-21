<script setup lang="ts">
// 404 PageNotFound Component
</script>

<template>
  <main class="not-found-container">
    <!-- Background Decorative Elements -->
    <div class="glow-sphere top-right"></div>
    <div class="glow-sphere bottom-left"></div>

    <div class="content">
      <!-- Massive Brutalist 404 -->
      <div class="massive-404-wrapper">
        <h1 class="display text-primary">404</h1>
        <div class="kinetic-overlay">
          <span class="display brutalist-outline">404</span>
        </div>
      </div>

      <!-- Message Section -->
      <div class="message-section">
        <div class="error-badge">
          <span class="label-sm text-secondary">Felmeddelande: Matchen saknas</span>
        </div>
        
        <h2 class="headline-lg text-on-surface">
          Hoppsan! Den här matchen verkar ha tagit slut eller aldrig börjat.
        </h2>
        
        <p class="body-lg text-on-surface-variant">
          Precis som en oväntad förlust i sista minuten, kunde vi inte hitta sidan du letade efter. Kanske har domaren blåst av i förtid?
        </p>

        <!-- Action Button -->
        <div class="action-wrapper">
          <router-link to="/" class="cta-button glow-primary">
            <span class="material-symbols-outlined">home</span>
            Tillbaka till startsidan
            <span class="material-symbols-outlined arrow">arrow_forward</span>
          </router-link>
        </div>
      </div>
    </div>

    <!-- Editorial Graphics -->
    <div class="editorial-card bottom-right lg-only">
      <div class="editorial-overlay"></div>
      <img src="https://lh3.googleusercontent.com/aida-public/AB6AXuDOFi9GBETkbISfWYj712JrRm8q8pC9ye5oaDePUo1icZKVaEsr0jlo6L0n4zZ_x2zuSnAf8Dy25zpWs0lS1Vtx9k4XzBYBK4olIXsPxfszYSa0nSm4x1fv-PExg9cZlHOG2dANRuuTj1EVFqTKuRZwqPetUtleGAC2_0qtBgqbyJ-0gvb52Ia7UD_YqnPN-1A1BP52_S3mX31YVJ8okfX0nVKHWD9C3bO756kquhLvowUhTqgdUB72tbCukDP1BFjFA68vQrKaFA" alt="Sports silhouette" />
      <div class="editorial-content">
        <div class="label-sm text-on-surface-variant">Status</div>
        <div class="headline-sm text-primary">EJ SPELAD</div>
      </div>
    </div>

    <div class="editorial-circle top-left lg-only">
      <div class="circle-inner">
        <img src="https://lh3.googleusercontent.com/aida-public/AB6AXuAZxiGx9XSL-1kHmhWnktQYbbsjlpmCls1aase0TShi5zoMs6V1MKCGatzWnePlPV_sTU1aNj5xC5TyCuzUzouMK0eQMOUunY-W4xbGp4Y63wyRXce1XwEdfnc4VKhik4Uw7FUuYGVbp7Bg4TCyvns-NjP8pXAVtYP74TTqQ-PdgfOl1DUCQQLBBjCTJBzI87ui_ctLlS3dD2j9bHBw_i1qQX5_Isg4R-_iuyp309K-XJFdU46b4rbuYNPhe7MEdHBTDaFXr3K8Ng" alt="Arena" />
      </div>
    </div>
  </main>
</template>

<style scoped>
.not-found-container {
  flex: 1;
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  padding: 48px 24px;
  position: relative;
  overflow: hidden;
  min-height: calc(100vh - 72px - 80px); /* Adjust for header/footer */
}

.glow-sphere {
  position: absolute;
  width: 400px;
  height: 400px;
  border-radius: var(--radius-full);
  filter: blur(120px);
  z-index: 0;
  pointer-events: none;
}

.top-right {
  top: -100px;
  right: -100px;
  background-color: rgba(108, 159, 255, 0.1);
}

.bottom-left {
  bottom: -100px;
  left: -100px;
  background-color: rgba(255, 215, 9, 0.05);
}

.content {
  max-width: 900px;
  width: 100%;
  text-align: center;
  z-index: 10;
}

.massive-404-wrapper {
  position: relative;
  display: inline-block;
  margin-bottom: 32px;
}

.kinetic-overlay {
  position: absolute;
  inset: 0;
  display: flex;
  align-items: center;
  justify-content: center;
  opacity: 0.2;
  pointer-events: none;
  user-select: none;
}

.kinetic-overlay span {
  transform: translate(16px, 16px);
}

.message-section {
  display: flex;
  flex-direction: column;
  gap: 24px;
  max-width: 640px;
  margin: 0 auto;
}

.error-badge {
  display: inline-block;
  padding: 6px 16px;
  background-color: var(--color-surface-container-high);
  border-radius: var(--radius-full);
  border: 1px solid rgba(70, 72, 75, 0.1);
  align-self: center;
}

.text-secondary { color: var(--color-secondary); }
.text-primary { color: var(--color-primary); }

.action-wrapper {
  margin-top: 40px;
}

.cta-button {
  display: inline-flex;
  align-items: center;
  gap: 12px;
  background-color: var(--color-primary);
  color: var(--color-on-primary-fixed);
  padding: 20px 40px;
  border-radius: var(--radius-full);
  font-family: 'Space Grotesk', sans-serif;
  font-weight: 900;
  font-size: 20px;
  text-decoration: none;
  transition: all 200ms ease-out;
}

.cta-button:hover {
  transform: scale(1.05);
}

.cta-button:active {
  transform: scale(0.95);
}

.cta-button .arrow {
  transition: transform 200ms ease-out;
}

.cta-button:hover .arrow {
  transform: translateX(4px);
}

/* Editorial Graphics */
.editorial-card {
  position: absolute;
  bottom: 48px;
  right: 48px;
  width: 256px;
  height: 320px;
  background-color: var(--color-surface-container-low);
  border-radius: var(--radius-3xl);
  border: 1px solid rgba(70, 72, 75, 0.2);
  transform: rotate(3deg);
  overflow: hidden;
  z-index: 5;
}

.editorial-overlay {
  position: absolute;
  inset: 0;
  background: linear-gradient(to top, var(--color-surface-container-low), transparent);
  z-index: 1;
}

.editorial-card img {
  width: 100%;
  height: 100%;
  object-fit: cover;
  opacity: 0.3;
  filter: grayscale(1);
}

.editorial-content {
  position: absolute;
  bottom: 16px;
  left: 24px;
  z-index: 2;
}

.editorial-circle {
  position: absolute;
  top: 48px;
  left: 48px;
  width: 192px;
  height: 192px;
  background-color: var(--color-surface-container-high);
  border-radius: var(--radius-full);
  border: 1px solid rgba(70, 72, 75, 0.1);
  padding: 4px;
  z-index: 5;
}

.circle-inner {
  width: 100%;
  height: 100%;
  border-radius: var(--radius-full);
  overflow: hidden;
  filter: grayscale(1);
  opacity: 0.4;
}

.circle-inner img {
  width: 100%;
  height: 100%;
  object-fit: cover;
}

/* Visibility */
.lg-only { display: none; }

@media (min-width: 1024px) {
  .lg-only { display: block; }
}

@media (max-width: 768px) {
  .headline-lg {
    font-size: 32px;
  }
}
</style>
