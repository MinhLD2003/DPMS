(function () {
  // Retrieve configuration from the <script> tag attributes
  const script = document.currentScript;
  const systemId = script.getAttribute("data-system-id");
  const uString = script.getAttribute("data-uString");
  const apiBase =
    script.getAttribute("data-dpms-api-base") ||
    "https://localhost:7226/api/Consent";

  // Key under which we cache consent per system
  const STORAGE_KEY = `dpmsConsent_${systemId}`;

  // Ensure required config
  if (!systemId || !uString) {
    console.error(
      "ConsentSDK: Missing required script attributes (data-system-id, data-uString)"
    );
    return;
  }
  // Cache helpers
  function cacheConsent(record) {
    console.log(record);
    localStorage.setItem(STORAGE_KEY, JSON.stringify(record));
  }

  // Get current in local
  function getCachedConsent() {
    const json = localStorage.getItem(STORAGE_KEY);
    return json ? JSON.parse(json) : null;
  }

  // SHA-256 hash(email + secretKey) SHOULD BE BACKEND HANDLE
  //   async function hashEmail(email, key) {
  //     const data = new TextEncoder().encode(email + key);
  //     const hashBuffer = await crypto.subtle.digest('SHA-256', data);
  //     return Array.from(new Uint8Array(hashBuffer))
  //       .map(b => b.toString(16).padStart(2, '0'))
  //       .join('');
  //   }

  // GET existing consent
  async function fetchConsent(emailHash, systemId) {
    const res = await fetch(`${apiBase}/banner/${emailHash}/${systemId}`, {
      credentials: "omit",
    });
    if (res.status === 200) return res.json();
    if (res.status === 404) return null;
    throw new Error(`DPMS GET failed with status ${res.status}`);
  }

  async function submitConsent({
    dataSubjectId,
    email,
    privacyPolicyId,
    externalSystemId,
    consentPurposes, // array of { purposeId: Guid, status: boolean }
    isWithdrawn = false,
    withdrawnDate = null, // or new Date().toISOString() if you want a timestamp
  }) {
    // auto‑detect IP (fallback to empty string)
    let consentIp = "";
    try {
      consentIp = await fetch("https://api.ipify.org?format=json")
        .then((r) => r.json())
        .then((j) => j.ip);
    } catch (_) {}

    const payload = {
      dataSubjectId, // string? (could be the hashed email)
      email, // string?
      consentMethod: "WebForm", // matches your ConsentMethod enum
      consentIp, // string
      consentUserAgent: navigator.userAgent,
      privacyPolicyId, // GUID
      externalSystemId, // GUID
      isWithdrawn, // bool?
      withdrawnDate, // DateTime?
      consentPurposes, // List< { purposeId, status } >
    };

    const res = await fetch(`${apiBase}/consent-js`, {
      method: "POST",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify(payload),
    });

    if (!res.ok) {
      throw new Error(`DPMS POST failed with status ${res.status}`);
    }

    const record = await res.json();
    cacheConsent(record);
    return record;
  }

  function injectStyles() {
    const style = document.createElement("style");
    style.innerHTML = `
          .consent-banner {
              position: fixed;
              top : 50%;
              left: 50%;
              transform: translate(-50%,-50%);
              width: 600px;
              height: 500px;
              background: #F3F3F3;
              border-radius: 5px;
              padding: 20px;
              box-shadow: 0px 4px 10px rgba(0, 0, 0, 0.2);
              font-family: Arial, sans-serif;
              z-index: 1000;
              color: #494949;
              display: flex;
              flex-direction: column;
  
          }
          .banner-content {
              display: flex;
              flex-direction: column;
              flex-grow: 1; /* Allows it to expand while keeping actions at the bottom */
              overflow: hidden;
          }
  
          .consent-banner * {
              max-width: 100%; /* Prevents elements from exceeding the container */
              box-sizing: border-box; /* Ensures padding/margins don't break layout */
              overflow-wrap: break-word; /* Prevents long text from overflowing */
          }
          .banner-description a {
              font-size: 11px;
          }
          .banner-description h3 {
              font-size: 16px;
              color: #494949;
              margin: 6px 0px;
          }
          .banner-description p {
              font-size: 14px;
              color: #494949;
              margin: 6px 0px;
          }
  
          .banner-header {
              display: flex;
              justify-content: space-between;
              align-items: center;
          }
  
          .logo {
              width: 100px;
          }
              
          .close-btn {
              background: none;
              border: none;
              font-size: 24px;
              cursor: pointer;
          }
  
          .purposes {
              flex-grow: 1;
              display: flex;
              flex-direction: column;
              padding: 2px;
              margin: 10px 0;
              height: auto; 
              overflow-y: auto; /* Enables scrolling only if content overflows */
              padding-right: 10px; /* Prevents content from being cut off */
          }
  
          .banner-purpose {
              display: flex;
              justify-content: space-between;
              align-items: center;
              background: #F3F3F3;
              font-size: 14px; 
              padding: 0px 10px;
              border-radius: 5px;
              margin-bottom: 10px;
              box-shadow: rgba(0, 0, 0, 0.16) 0px 1px 4px;
              width: 100%;
              min-height: auto;
              flex-wrap: nowrap; /* Prevents switch from wrapping */
          }
          .banner-purpose div {
              flex-grow: 1;
              min-width: 0;
              word-break: break-word; /* Ensures text wraps properly */
              padding-right: 10px; /* Creates space between text and switch */
              margin-bottom: 10px;
          }
          .banner-purpose div h4, p{
              margin: 10px 0 0 0;
          }
  
          .switch {
              flex-shrink: 0;
              position: relative;
              display: inline-block;
              width: 34px;
              height: 20px;
              margin-left: 10px;
              align-self: center;
          }
  
          .switch input {
              opacity: 0;
              width: 0;
              height: 0;
          }
  
          .slider {
              position: absolute;
              cursor: pointer;
              top: 0;
              left: 0;
              right: 0;
              bottom: 0;
              background-color: #ccc;
              border-radius: 20px;
              transition: 0.4s;
          }
  
          .slider:before {
              position: absolute;
              content: "";
              height: 14px;
              width: 14px;
              left: 3px;
              bottom: 3px;
              background-color: white;
              border-radius: 50%;
              transition: 0.4s;
          }
  
          input:checked + .slider {
              background-color: #4caf50;
          }
  
          input:checked + .slider:before {
              transform: translateX(14px);
          }
  
          .banner-actions {
              flex: none;
              display: flex;
              justify-content: space-between;
              gap: 10px;
          }
  
          .agree-btn, .reject-btn {
              flex: 1;
              align-text: center;
              padding: 12px 2px;
              border: none;
              border-radius: 5px;
              cursor: pointer;
              font-size: 16px;
              font-weight: normal;
              background: #0066B2;
              color: #ccc;
          }
  
          .agree-btn:hover {
              background: #0DB04B;
              color: white;
          }
          .reject-btn:hover {
              background: #F26F21;
              color: white;
          }
  
          .powered-by {
              text-align: center;
              margin: 10px 0 0 0;
              font-size: 11px;
          }
          .divider {
              border: none;
              height: 1px !important;
              background: #ccc; /* Light gray line */
              margin: 10px 0;
          }
          /* Optional scrollbar styling */
          .purposes::-webkit-scrollbar {
              width: 5px;
          }
  
          .purposes::-webkit-scrollbar-thumb {
              background-color: #fff;
              border-radius: 10px;
          }
      `;
    document.head.appendChild(style);
  }

  function renderConsentBanner(data) {
    // Inject styles
    injectStyles();

    // remove any old open‑icon
    const oldIcon = document.querySelector(".open-consent-icon");
    if (oldIcon) oldIcon.remove();

    // Create the consent banner container
    const consentBanner = document.createElement("div");
    consentBanner.classList.add("consent-banner");
    consentBanner.innerHTML = `
          <div class="banner-header">
              <img src="https://fpt.edu.vn/Content/images/assets/img-logo-fe.png" alt="FPT Education" class="logo">
              <button class="close-btn">&times;</button>
          </div>
          <hr class="divider">
          <div class="banner-content">
              <div class="banner-description">
                  <h3>Cài đặt</h3>
                  <p>Nothing</p>
                  <a href="#">google.com</a>    
              </div>
              <hr class="divider">
              <div class="purposes">
                  ${data
                    .map((purpose) => {
                      return `
                      <div class="banner-purpose">
                          <div>
                          <h4>${purpose.purposeName}</h4>
                          <p>${purpose.purposeDescription}</p>
                          </div>
                          <label class="switch">
                          <input type="checkbox"
                          data-purpose="${slugify(p.purposeName)}"
                          data-purpose-id="${p.purposeId}"
                          ${p.status === "True" ? "checked" : ""}>
                          <span class="slider"></span>
                          </label>
                      </div>
                      `;
                    })
                    .join("")}
              </div>
              <hr class="divider">
              <div class="banner-actions">
                  <button class="agree-btn">Gửi sự đồng ý</button>
                  <button class="reject-btn">Từ chối</button>
              </div>
  
              <p class="powered-by">powered by <strong>DPMS</strong></p>
          </div>
      `;

    document.body.appendChild(consentBanner);

    // Close button
    consentBanner.querySelector(".close-btn").addEventListener("click", () => {
      consentBanner.remove();
      showOpenIcon();
    });

    const agreeBtn = consentBanner.querySelector(".agree-btn");
    const rejectBtn = consentBanner.querySelector(".reject-btn");

    agreeBtn.addEventListener("click", async () => {
      // build your list of ConsentPurposeVM objects
      const consentPurposes = Array.from(
        consentBanner.querySelectorAll('input[type="checkbox"]')
      ).map((cb) => ({
        purposeId: cb.dataset.purposeId, // GUID string from your checkbox
        status: cb.checked, // boolean
      }));

      try {
        await submitConsent({
          dataSubjectId: uString,
          email: uString,
          privacyPolicyId: crypto.randomUUID(),
          externalSystemId: systemId,
          consentPurposes, // the array you just built
          isWithdrawn: false,
          withdrawnDate: null,
        });
      } catch (err) {
        console.error("Consent submit failed:", err);
      } finally {
        consentBanner.remove();
        showOpenIcon();
      }
    });

    rejectBtn.addEventListener("click", async () => {
      // build your list of ConsentPurposeVM objects
      const consentPurposes = Array.from(
        consentBanner.querySelectorAll('input[type="checkbox"]')
      ).map((cb) => ({
        purposeId: cb.dataset.purposeId, // GUID string from your checkbox
        status: false, // boolean
      }));

      try {
        await submitConsent({
          dataSubjectId: uString,
          email: uString,
          privacyPolicyId: crypto.randomUUID(),
          externalSystemId: systemId,
          consentPurposes, // the array you just built
          isWithdrawn: false,
          withdrawnDate: null,
        });
      } catch (err) {
        console.error("Consent submit failed:", err);
      } finally {
        consentBanner.remove();
        showOpenIcon();
      }
    });
  }

  // Reopnen Consent banner
  async function openConsentBanner() {
    // Always fetch the current consent state (ignore any cached flag)
    let existing = getCachedConsent();
    // 2. If we don’t have anything locally, fetch from the API
    if (!existing) {
      try {
        existing = await fetchConsent(uString, systemId);
        // cache what we fetched (could be null too, but then payload uses systemPurpose)
        if (existing) {
          cacheConsent(existing);
        }
      } catch (err) {
        console.error("Error fetching consent for manual open:", err);
        // if fetch fails, we can still render the banner with default purposes
      }
    }
    // 3. Render, passing in the current state
    renderConsentBanner(existing);
  }

  function showOpenIcon() {
    // don’t duplicate
    if (document.querySelector(".open-consent-icon")) return;

    const icon = document.createElement("div");
    icon.classList.add("open-consent-icon");
    // SVG chat‑bubble icon
    icon.innerHTML = `
          <svg viewBox="0 0 24 24" aria-hidden="true">
              <path d="M4 4h16v12H5.17L4 17.17V4zm2 2v8h12V6H6z"/>
          </svg>
          `;
    icon.addEventListener("click", openConsentBanner);
    document.body.appendChild(icon);
  }

  // 4) Init on page load
  (async function init() {
    const cached = getCachedConsent();
    if (cached) {
      const needsConsent = cached.some((p) => p.Status === null);
      if (needsConsent) {
        renderConsentBanner(cached);
      } else {
        cacheConsent(cached);
        showOpenIcon();
      }
      return;
    }

    try {
      const fetched = await fetchConsent(uString, systemId);
      if (fetched) {
        cacheConsent(fetched);
        const needsConsent = fetched.some((p) => p.Status === null);
        if (needsConsent) {
          renderConsentBanner(fetched);
        } else {
          showOpenIcon();
        }
      } else {
        renderConsentBanner(fetched);
      }
    } catch (err) {
      console.error("Error fetching consent:", err);
      showOpenIcon();
    }
  })();
  //   // Initialization
  //   (async function init() {
  //     const cached = getCachedConsent();
  //     if (cached) return;

  //     let existing;
  //     try {
  //       existing = await fetchConsent(uString, systemId);
  //     } catch (err) {
  //       console.error('Error fetching consent:', err);
  //       return;
  //     }

  //     if (existing) {
  //         // check if *any* purpose still needs consent
  //         const needsConsent = existing.some(p => p.Status === null);
  //         if (needsConsent) {
  //         // you can pass only the pending ones, or the whole list
  //         renderConsentBanner(existing);
  //         } else {
  //         // user has opted in or out on *every* purpose
  //         cacheConsent(existing);
  //         showOpenIcon();
  //         }
  //       cacheConsent(existing);
  //     } else {
  //         console.error('Error fetching consent: uString oke but no consent');
  //         showOpenIcon();
  //     }
  //   })();
})();
