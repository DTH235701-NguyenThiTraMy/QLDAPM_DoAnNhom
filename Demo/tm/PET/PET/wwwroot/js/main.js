/* ==========================================================================
   Fetch — vanilla JavaScript (no jQuery)
   One function per feature · guard clauses · IntersectionObserver
   Honours prefers-reduced-motion · progressive enhancement
   ========================================================================== */
(function () {
  'use strict';

  var reduceMotion = window.matchMedia('(prefers-reduced-motion: reduce)').matches;

  /* ---------------------------------------------------------- Sticky nav */
  function initNavScroll() {
    var nav = document.querySelector('.site-nav');
    if (!nav) return;
    var onScroll = function () {
      nav.classList.toggle('scrolled', window.scrollY > 20);
    };
    onScroll();
    window.addEventListener('scroll', onScroll, { passive: true });
  }

  /* ------------------------------------------------ Mobile menu close */
  function initMobileMenu() {
    var collapseEl = document.getElementById('mainNav');
    if (!collapseEl) return;
    // Close the Bootstrap collapse when a link is tapped
    collapseEl.querySelectorAll('.nav-link, .btn').forEach(function (link) {
      link.addEventListener('click', function () {
        if (window.innerWidth >= 992) return;
        var inst = window.bootstrap && window.bootstrap.Collapse.getInstance(collapseEl);
        if (inst) inst.hide();
      });
    });
  }

  /* --------------------------------------------------- Scrollspy (active link) */
  function initScrollSpy() {
    var links = Array.prototype.slice.call(document.querySelectorAll('[data-spy]'));
    if (!links.length) return;
    var sections = links
      .map(function (l) { return document.querySelector(l.getAttribute('href')); })
      .filter(Boolean);
    if (!sections.length) return;

    var spy = new IntersectionObserver(function (entries) {
      entries.forEach(function (e) {
        if (!e.isIntersecting) return;
        links.forEach(function (l) {
          l.classList.toggle('active', l.getAttribute('href') === '#' + e.target.id);
        });
      });
    }, { rootMargin: '-45% 0px -50% 0px', threshold: 0 });

    sections.forEach(function (s) { spy.observe(s); });
  }

  /* --------------------------------------------------------- Reveal on scroll */
  function initReveal() {
    var items = document.querySelectorAll('.reveal');
    if (!items.length) return;
    if (reduceMotion || !('IntersectionObserver' in window)) {
      items.forEach(function (el) { el.classList.add('in'); });
      return;
    }
    var io = new IntersectionObserver(function (entries, obs) {
      entries.forEach(function (e) {
        if (e.isIntersecting) { e.target.classList.add('in'); obs.unobserve(e.target); }
      });
    }, { threshold: 0.12, rootMargin: '0px 0px -8% 0px' });
    items.forEach(function (el) { io.observe(el); });
  }

  /* --------------------------------------------------------- Count-up stats */
  function initCountUp() {
    var nums = document.querySelectorAll('[data-count]');
    if (!nums.length) return;

    var run = function (el) {
      var target = parseFloat(el.getAttribute('data-count'));
      var suffix = el.getAttribute('data-suffix') || '';
      var decimals = (target % 1 !== 0) ? 1 : 0;
      if (reduceMotion) { el.textContent = target.toFixed(decimals) + suffix; return; }
      var dur = 1400, start = null;
      var step = function (ts) {
        if (!start) start = ts;
        var p = Math.min((ts - start) / dur, 1);
        var eased = 1 - Math.pow(1 - p, 3);
        el.textContent = (target * eased).toFixed(decimals) + suffix;
        if (p < 1) requestAnimationFrame(step);
        else el.textContent = target.toFixed(decimals) + suffix;
      };
      requestAnimationFrame(step);
    };

    if (!('IntersectionObserver' in window)) { nums.forEach(run); return; }
    var io = new IntersectionObserver(function (entries, obs) {
      entries.forEach(function (e) {
        if (e.isIntersecting) { run(e.target); obs.unobserve(e.target); }
      });
    }, { threshold: 0.5 });
    nums.forEach(function (n) { io.observe(n); });
  }

  /* --------------------------------------------------- Before/After slider */
  function initBeforeAfter() {
    document.querySelectorAll('.ba-slider').forEach(function (slider) {
      var afterWrap = slider.querySelector('.ba-after-wrap');
      var handle = slider.querySelector('.ba-handle');
      var afterImg = afterWrap && afterWrap.querySelector('img');
      if (!afterWrap || !handle) return;
      var pos = 50;

      var setWidthImg = function () {
        if (afterImg) afterImg.style.width = slider.clientWidth + 'px';
      };
      var apply = function (pct) {
        pos = Math.max(0, Math.min(100, pct));
        afterWrap.style.width = pos + '%';
        handle.style.left = pos + '%';
        handle.setAttribute('aria-valuenow', Math.round(pos));
      };
      var fromClientX = function (clientX) {
        var rect = slider.getBoundingClientRect();
        apply(((clientX - rect.left) / rect.width) * 100);
      };

      var dragging = false;
      var start = function (e) { dragging = true; e.preventDefault(); };
      var move = function (e) {
        if (!dragging) return;
        var x = e.touches ? e.touches[0].clientX : e.clientX;
        fromClientX(x);
      };
      var end = function () { dragging = false; };

      handle.addEventListener('mousedown', start);
      handle.addEventListener('touchstart', start, { passive: false });
      window.addEventListener('mousemove', move);
      window.addEventListener('touchmove', move, { passive: false });
      window.addEventListener('mouseup', end);
      window.addEventListener('touchend', end);
      // click anywhere on slider to jump
      slider.addEventListener('click', function (e) {
        if (e.target === handle || handle.contains(e.target)) return;
        fromClientX(e.clientX);
      });
      // keyboard
      handle.addEventListener('keydown', function (e) {
        if (e.key === 'ArrowLeft') { apply(pos - 4); e.preventDefault(); }
        else if (e.key === 'ArrowRight') { apply(pos + 4); e.preventDefault(); }
        else if (e.key === 'Home') { apply(0); e.preventDefault(); }
        else if (e.key === 'End') { apply(100); e.preventDefault(); }
      });

      window.addEventListener('resize', setWidthImg);
      setWidthImg();
      apply(50);
    });
  }

  /* --------------------------------------------------------- Lightbox gallery */
  function initLightbox() {
    var triggers = Array.prototype.slice.call(document.querySelectorAll('[data-lightbox]'));
    var box = document.getElementById('lightbox');
    if (!triggers.length || !box) return;

    var imgEl = box.querySelector('.lb-img');
    var capEl = box.querySelector('.lb-cap');
    var current = 0;
    var lastFocus = null;

    var items = triggers.map(function (t) {
      return { src: t.getAttribute('data-lightbox'), cap: t.getAttribute('data-caption') || '' };
    });

    var show = function (i) {
      current = (i + items.length) % items.length;
      imgEl.src = items[current].src;
      imgEl.alt = items[current].cap;
      if (capEl) capEl.textContent = items[current].cap;
    };
    var open = function (i) {
      lastFocus = document.activeElement;
      show(i);
      box.classList.add('open');
      box.setAttribute('aria-hidden', 'false');
      document.body.style.overflow = 'hidden';
      box.querySelector('.lb-close').focus();
    };
    var close = function () {
      box.classList.remove('open');
      box.setAttribute('aria-hidden', 'true');
      document.body.style.overflow = '';
      if (lastFocus) lastFocus.focus();
    };

    triggers.forEach(function (t, i) {
      t.addEventListener('click', function (e) { e.preventDefault(); open(i); });
    });
    box.querySelector('.lb-close').addEventListener('click', close);
    box.querySelector('.lb-next').addEventListener('click', function () { show(current + 1); });
    box.querySelector('.lb-prev').addEventListener('click', function () { show(current - 1); });
    box.addEventListener('click', function (e) { if (e.target === box) close(); });
    document.addEventListener('keydown', function (e) {
      if (!box.classList.contains('open')) return;
      if (e.key === 'Escape') close();
      else if (e.key === 'ArrowRight') show(current + 1);
      else if (e.key === 'ArrowLeft') show(current - 1);
    });
  }

  /* --------------------------------------------------- Testimonials slider */
  function initTestimonials() {
    var root = document.querySelector('[data-tst]');
    if (!root) return;
    var track = root.querySelector('.tst-track');
    var slides = root.querySelectorAll('.tst-slide');
    var prev = root.querySelector('.tst-prev');
    var next = root.querySelector('.tst-next');
    var dotsWrap = root.querySelector('.tst-dots');
    if (!track || !slides.length) return;

    var index = 0;

    var perView = function () {
      if (window.innerWidth >= 992) return 3;
      if (window.innerWidth >= 768) return 2;
      return 1;
    };
    var maxIndex = function () { return Math.max(0, slides.length - perView()); };

    var buildDots = function () {
      if (!dotsWrap) return;
      dotsWrap.innerHTML = '';
      var pages = maxIndex() + 1;
      for (var i = 0; i < pages; i++) {
        var d = document.createElement('button');
        d.type = 'button';
        d.className = 'tst-dot' + (i === index ? ' active' : '');
        d.setAttribute('aria-label', 'Go to review ' + (i + 1));
        (function (n) { d.addEventListener('click', function () { go(n); }); })(i);
        dotsWrap.appendChild(d);
      }
    };
    var update = function () {
      var pct = index * (100 / perView());
      track.style.transform = 'translateX(-' + pct + '%)';
      if (dotsWrap) {
        dotsWrap.querySelectorAll('.tst-dot').forEach(function (d, i) {
          d.classList.toggle('active', i === index);
        });
      }
    };
    var go = function (i) { index = Math.max(0, Math.min(maxIndex(), i)); update(); };

    if (next) next.addEventListener('click', function () {
      index = index >= maxIndex() ? 0 : index + 1; update();
      if (dotsWrap) buildDots();
    });
    if (prev) prev.addEventListener('click', function () {
      index = index <= 0 ? maxIndex() : index - 1; update();
      if (dotsWrap) buildDots();
    });

    var rt;
    window.addEventListener('resize', function () {
      clearTimeout(rt);
      rt = setTimeout(function () { go(Math.min(index, maxIndex())); buildDots(); }, 150);
    });

    buildDots();
    update();

    // autoplay (pauses on hover / reduced motion off)
    if (!reduceMotion) {
      var timer = setInterval(function () {
        index = index >= maxIndex() ? 0 : index + 1; update();
        buildDots();
      }, 6000);
      root.addEventListener('mouseenter', function () { clearInterval(timer); });
    }
  }

  /* ------------------------------------------------- Service filter (tabs) */
  function initServiceFilter() {
    var wrap = document.querySelector('[data-filter-group]');
    if (!wrap) return;
    var buttons = wrap.querySelectorAll('[data-filter]');
    var cards = document.querySelectorAll('[data-cat]');
    if (!buttons.length || !cards.length) return;

    buttons.forEach(function (btn) {
      btn.addEventListener('click', function () {
        var f = btn.getAttribute('data-filter');
        buttons.forEach(function (b) {
          b.classList.toggle('btn-paw', b === btn);
          b.classList.toggle('btn-ghost', b !== btn);
          b.setAttribute('aria-pressed', b === btn ? 'true' : 'false');
        });
        cards.forEach(function (card) {
          var cat = card.getAttribute('data-cat');
          var show = (f === 'all' || cat === f);
          card.style.display = show ? '' : 'none';
        });
      });
    });
  }

  /* --------------------------------- Booking form: chips + estimate + validation */
  function initBooking() {
    var form = document.getElementById('bookingForm');
    if (!form) return;

    var okPanel = document.getElementById('bookingOk');
    var estEl = document.getElementById('estPrice');
    var estTime = document.getElementById('estTime');
    var steps = document.querySelectorAll('.step-pill');

    // Pricing matrix: [service] -> { base by size, minutes }
    var SERVICES = {
      'bath-brush':  { name: 'Bath & Brush',  s: 30, m: 40, l: 55, xl: 70,  min: 45 },
      'full-groom':  { name: 'Full Groom',    s: 48, m: 62, l: 82, xl: 105, min: 90 },
      'nail-trim':   { name: 'Nail Trim',     s: 15, m: 15, l: 18, xl: 20,  min: 20 },
      'deshedding':  { name: 'De-shedding',   s: 42, m: 55, l: 72, xl: 92,  min: 75 },
      'puppy':       { name: 'Puppy Groom',   s: 35, m: 42, l: 52, xl: 60,  min: 50 },
      'cat':         { name: 'Cat Grooming',  s: 55, m: 65, l: 78, xl: 78,  min: 70 }
    };

    var getVal = function (name) {
      var el = form.querySelector('input[name="' + name + '"]:checked');
      return el ? el.value : '';
    };

    var updateEstimate = function () {
      var size = getVal('size');
      var svc = form.querySelector('#svcSelect') ? form.querySelector('#svcSelect').value : '';
      if (SERVICES[svc] && size && SERVICES[svc][size] != null) {
        var price = SERVICES[svc][size];
        if (estEl) estEl.textContent = '$' + price;
        if (estTime) estTime.textContent = '~' + SERVICES[svc].min + ' min';
      } else {
        if (estEl) estEl.textContent = '—';
        if (estTime) estTime.textContent = 'Pick a service';
      }
      updateSteps();
    };

    var updateSteps = function () {
      if (!steps.length) return;
      var s1 = getVal('pet') && getVal('size');
      var svc = form.querySelector('#svcSelect');
      var s2 = svc && svc.value;
      var date = form.querySelector('#dateField');
      var time = form.querySelector('#timeField');
      var s3 = date && date.value && time && time.value;
      var state = [s1, s2, s3];
      steps.forEach(function (pill, i) {
        pill.classList.toggle('done', !!state[i]);
        pill.classList.toggle('current', !state[i] && (i === 0 || state[i - 1]));
      });
    };

    form.addEventListener('change', updateEstimate);
    form.addEventListener('input', updateSteps);

    // Set date min to today
    var dateField = form.querySelector('#dateField');
    if (dateField) {
      var t = new Date();
      dateField.min = t.toISOString().split('T')[0];
    }

    var clearError = function (group) { group.classList.remove('field-error'); };

    var validate = function () {
      var ok = true;
      var firstBad = null;

      var groups = form.querySelectorAll('[data-required-group]');
      groups.forEach(function (g) {
        var name = g.getAttribute('data-required-group');
        var checked = form.querySelector('input[name="' + name + '"]:checked');
        if (!checked) { g.classList.add('field-error'); ok = false; if (!firstBad) firstBad = g; }
        else clearError(g);
      });

      form.querySelectorAll('[data-required]').forEach(function (field) {
        var wrapEl = field.closest('.form-group') || field.parentElement;
        var valid = field.value.trim() !== '';
        if (field.type === 'email' && valid) {
          valid = /^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(field.value.trim());
        }
        wrapEl.classList.toggle('field-error', !valid);
        if (!valid) { ok = false; if (!firstBad) firstBad = field; }
      });

      if (firstBad && firstBad.focus) firstBad.focus();
      else if (firstBad) {
        var f = firstBad.querySelector('input, select');
        if (f) f.focus();
      }
      return ok;
    };

    // Clear errors as the user fixes fields
    form.addEventListener('change', function (e) {
      var g = e.target.closest('[data-required-group]');
      if (g) clearError(g);
    });
    form.querySelectorAll('[data-required]').forEach(function (field) {
      field.addEventListener('input', function () {
        var wrapEl = field.closest('.form-group') || field.parentElement;
        if (field.value.trim() !== '') wrapEl.classList.remove('field-error');
      });
    });

    form.addEventListener('submit', function (e) {
      e.preventDefault();
      if (!validate()) return;
      if (okPanel) {
        var svcSel = form.querySelector('#svcSelect');
        var svcName = svcSel && SERVICES[svcSel.value] ? SERVICES[svcSel.value].name : 'appointment';
        var nameField = form.querySelector('#nameField');
        var petName = nameField ? nameField.value.trim() : '';
        var confirmName = document.getElementById('okName');
        if (confirmName) confirmName.textContent = petName ? petName : 'your pet';
        var confirmSvc = document.getElementById('okService');
        if (confirmSvc) confirmSvc.textContent = svcName;
        form.style.display = 'none';
        okPanel.classList.add('show');
        okPanel.scrollIntoView({ behavior: reduceMotion ? 'auto' : 'smooth', block: 'center' });
      }
    });

    var resetBtn = document.getElementById('bookAnother');
    if (resetBtn) resetBtn.addEventListener('click', function () {
      form.reset();
      form.style.display = '';
      if (okPanel) okPanel.classList.remove('show');
      updateEstimate();
    });

    updateEstimate();
  }

  /* ------------------------------------ Prefill service from "Book this" */
  function initServicePrefill() {
    var select = document.getElementById('svcSelect');
    if (!select) return;
    document.querySelectorAll('[data-prefill]').forEach(function (btn) {
      btn.addEventListener('click', function () {
        var val = btn.getAttribute('data-prefill');
        if (!select.querySelector('option[value="' + val + '"]')) return;
        select.value = val;
        // notify listeners so the estimate + step pills update
        select.dispatchEvent(new Event('change', { bubbles: true }));
      });
    });
  }

  /* ------------------------------------------------- Newsletter (footer) */
  function initNewsletter() {
    document.querySelectorAll('[data-newsletter]').forEach(function (form) {
      form.addEventListener('submit', function (e) {
        e.preventDefault();
        var input = form.querySelector('input[type="email"]');
        var note = form.querySelector('[data-note]');
        if (!input) return;
        var valid = /^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(input.value.trim());
        if (!valid) {
          if (note) { note.textContent = 'Please enter a valid email.'; note.style.color = '#ff7a68'; }
          input.focus();
          return;
        }
        if (note) { note.textContent = 'You’re on the list — welcome to the pack!'; note.style.color = '#ffc93c'; }
        form.reset();
      });
    });
  }

  /* --------------------------------------------------------- Footer year */
  function initYear() {
    var el = document.getElementById('year');
    if (el) el.textContent = new Date().getFullYear();
  }

  /* --------------------------------------------------------- Init */
  document.addEventListener('DOMContentLoaded', function () {
    initNavScroll();
    initMobileMenu();
    initScrollSpy();
    initReveal();
    initCountUp();
    initBeforeAfter();
    initLightbox();
    initTestimonials();
    initServiceFilter();
    initBooking();
    initServicePrefill();
    initNewsletter();
    initYear();
  });
})();
